using dotbak.Models;
using dotbak.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

namespace dotbak.Services
{
    public class ServerInfoServices
    {
        private SqlConnection sqlConnection;
        private SqlCommand cmd;
        private SqlDataReader dr;
        public ServerInfoServices()
        {
        }
        private void OpenConnection()
        {
            string connectionString = $@"Data Source= {Constants.machineName};Database={Constants.defaultDatebase};Integrated Security=true;";
            sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
        }
        public Server GetServers()
        {
            OpenConnection();
            cmd = new SqlCommand("SELECT srvname, srvproduct, providername FROM sysservers", sqlConnection);
            dr = cmd.ExecuteReader();
            Server server = new Server();
            while (dr.Read())
            {
                server.ServerName = (string)dr[0];
                server.ProviderType = (string)dr[1];
                server.ProviderName = (string)dr[2];
            }
            dr.Close();
            CloseConnection();
            return server;
        }
        public List<string> GetDatabases()
        {
            OpenConnection();
            string where = $@"WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb')";
            string query = $@"SELECT name, filename, crdate FROM sysdatabases {where}";
            cmd = new SqlCommand(query, sqlConnection);
            dr = cmd.ExecuteReader();
            List<string> databaseNames = new List<string>();
            while (dr.Read())
            {
                databaseNames.Add((string)dr[0]);
            }
            dr.Close();
            CloseConnection();
            return databaseNames;
        }
        public ServerDatabase GetDatabaseInfoByName(string databaseName)
        {
            OpenConnection();
            string where = $@"WHERE name = '{databaseName}'";
            string query = $@"SELECT filename, crdate FROM sysdatabases {where}";
            cmd = new SqlCommand(query, sqlConnection);
            dr = cmd.ExecuteReader();
            ServerDatabase database = new ServerDatabase();
            while (dr.Read())
            {
                database.DatabaseLocation = (string)dr[0];
                database.CreatedDate = (DateTime)dr[1];
            }
            dr.Close();
            CloseConnection();
            return database;
        }
        public async Task<string> CreateBackup(string path, string databaseName)
        {
            OpenConnection();
            string msg = string.Empty;
            string query = $@"BACKUP DATABASE {databaseName} TO DISK = '{path}'";
            cmd = new SqlCommand(query, sqlConnection);
            cmd.CommandTimeout = 60 * 60;
            int result = await cmd.ExecuteNonQueryAsync();
            if (result == 0)
                msg = "Database Backup Failed!";
            else
                msg = "BackUp Successed!";
            CloseConnection();
            return msg;
        }
        private void CloseConnection()
        {
            if (sqlConnection.State == ConnectionState.Open)
                sqlConnection.Close();
        }
    }
}
