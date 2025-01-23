using System;
using System.Data;
using System.Linq;
using Moduli_Rilascio_Core;
using MySql.Data.MySqlClient;
using VatService.Structure;

public class DBWriter : IOperations
{
    private MySqlConnection m_connection;
    private string m_connectionString;
    private DataSet m_dataSet; //ricopia istanza del database
    private DataSet m_externalDataset; //contiene le nuove occorrenze verificate

    public DBWriter(ref IniUtility iniUtility, ref DataSet dataSet)
    {
        m_connectionString = string.Format("Server=localhost;Database=VatRegister;Uid={0};Pwd={1};", iniUtility.Read("username"), iniUtility.Read("password"));
        m_dataSet = new DataSet();
        m_externalDataset = dataSet;

        //dbOperations
        Connect();
        InitDatabase();
    }

    internal void InitDatabase()
    {
        // Verifica se la tabella esiste già nel database
        string tableCheckQuery = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'vatregister' AND table_name = 'StoryVat';";
        MySqlCommand tableCheckCommand = new MySqlCommand(tableCheckQuery, m_connection);
        int tableCount = Convert.ToInt32(tableCheckCommand.ExecuteScalar());

        // Se la tabella non esiste, inizializzo
        if (tableCount == 0)
        {
            string createTableQuery = @"
                                        CREATE TABLE StoryVat (
                                        countryCode VARCHAR(2),
                                        vatNumber VARCHAR(20),
                                        requestDate VARCHAR(30),
                                        valid BOOLEAN,
                                        name VARCHAR(255),
                                        address VARCHAR(255),
                                        PRIMARY KEY (vatNumber, countryCode)
                                        );
                                        ";

            MySqlCommand createTableCommand = new MySqlCommand(createTableQuery, m_connection);
            createTableCommand.ExecuteNonQuery();
        }
    }
    public void GetBulk(ref DataSet dataSet) //restituisco un database copiato per schema
    {
        MySqlDataAdapter dataAdapter = new MySqlDataAdapter();
        MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM StoryVat", m_connection);
        dataAdapter.SelectCommand = selectCommand;
        dataAdapter.FillSchema(dataSet, SchemaType.Source, "StoryVat");
    }
    public void WriteOnDB()
    {
        try
        {
            MySqlTransaction transaction = m_connection.BeginTransaction();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter();
            MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM StoryVat", m_connection, transaction);
            dataAdapter.SelectCommand = selectCommand;

            // Utilizzo di MySqlCommandBuilder per generare automaticamente i comandi di inserimento, aggiornamento e cancellazione
            MySqlCommandBuilder commandBuilder = new MySqlCommandBuilder(dataAdapter);

            // Esecuzione della query per riempire il DataSet all'interno della transazione
            m_dataSet.Clear(); //pulizia
            dataAdapter.FillSchema(m_dataSet, SchemaType.Source,"StoryVat"); //copio il bulk del db e le chiavi
            dataAdapter.Fill(m_dataSet, "StoryVat"); //copio i dati presenti nel db

            m_dataSet.EnforceConstraints = true; //assicuro i vincoli

            foreach (DataRow row in m_externalDataset.Tables["StoryVat"].Rows)
            {
                if (m_dataSet.Tables["StoryVat"].AsEnumerable().Any(x => x["vatNumber"].Equals(row["vatNumber"].ToString())))
                {
                    //update operation
                    DataRow rowUpdated = m_dataSet.Tables["StoryVat"].AsEnumerable().FirstOrDefault(x => x["vatNumber"].Equals(row["vatNumber"].ToString()));
                    //rowUpdated["countryCode"] = row["countryCode"];
                    //rowUpdated["vatNumber"] = row["vatNumber"];
                    rowUpdated["requestDate"] = row["requestDate"];
                    rowUpdated["valid"] = row["valid"];
                    rowUpdated["name"] = row["name"];
                    rowUpdated["address"] = row["address"];
                }
                else
                {
                    //insert operation
                    DataRow newRow = m_dataSet.Tables["StoryVat"].NewRow();
                    newRow["countryCode"] = row["countryCode"];
                    newRow["vatNumber"] = row["vatNumber"];
                    newRow["requestDate"] = row["requestDate"];
                    newRow["valid"] = row["valid"];
                    newRow["name"] = row["name"];
                    newRow["address"] = row["address"];

                    // Aggiunta del nuovo record al DataSet
                    m_dataSet.Tables["StoryVat"].Rows.Add(newRow);
                }
            }


            // Applicazione dei cambiamenti al database
            dataAdapter.Update(m_dataSet, "StoryVat");


            // Commit della transazione
            MySqlCommand commitTransactionCommand = m_connection.CreateCommand();
            commitTransactionCommand.CommandText = "COMMIT;";
            commitTransactionCommand.ExecuteNonQuery();

            Console.WriteLine("Transazione completata con successo.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Errore durante l'esecuzione della transazione: " + ex.Message);

            // Rollback della transazione in caso di errore
            MySqlCommand rollbackTransactionCommand = m_connection.CreateCommand();
            rollbackTransactionCommand.CommandText = "ROLLBACK;";
            rollbackTransactionCommand.ExecuteNonQuery();
        }

    }
    public void Connect()
    {
        if (m_connection is null || m_connection.State != ConnectionState.Open)
        {
            m_connection = new MySqlConnection(m_connectionString);
            m_connection.Open();
        }
    }
    public void Dispose()
    {
        m_connection.Close();
        m_connection.Dispose();
    }
}
