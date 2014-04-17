using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MessageImporter.Entities
{
    /// <summary>
    /// Base DB entity with basic columns
    /// </summary>
    [Serializable]
    public abstract class BaseEntity<T> : IEntity
        where T : BaseEntity<T>, new()
    {
        // column variables for mapping
        internal string Comment { get; set; }
        internal long? Id { get; set; }
        // internal because of invisibility in grids
        internal bool Valid { get; set; }

        // basic column names
        public static string ID = "ID";
        public static string COMMENT = "COMMENT";
        public static string VALID = "VALID";

        /// <summary>
        /// Constructor with table name of the entity..
        /// </summary>
        public BaseEntity()
        {
            Clear();
        }

        /// <summary>
        /// Clears the basic properties of the entity
        /// </summary>
        public virtual void Clear()
        {
            Id = null;
            Comment = string.Empty;
            Valid = true;
        }

        /// <summary>
        /// Creates empty entity
        /// </summary>
        public static T Empty
        {
            get
            {
                return new T();
            }
        }

        /// <summary>
        /// Loads all data from table where VALID = 1
        /// </summary>
        /// <returns></returns>
        public static DataTable LoadAllValid_DT(string tableName)
        {
            var ds = DBProvider.ExecuteQuery(string.Format("select * from {0} where {1} = {2}", tableName, VALID, 1));
            if (ds == null || ds.Tables == null || ds.Tables.Count == 0)
                return new DataTable();

            return ds.Tables[0];
        }

        /// <summary>
        /// Loads all data from the entity table
        /// </summary>
        /// <returns></returns>
        public static DataTable LoadAll_DT(string tableName)
        {
            var ds = DBProvider.ExecuteQuery(string.Format("select * from {0}", tableName));
            if (ds == null || ds.Tables == null || ds.Tables.Count == 0)
                return new DataTable();

            return ds.Tables[0];
        }

        public static List<T> LoadAll(string tableName)
        {
            var ret = new List<T>();
            var table = LoadAllValid_DT(tableName);

            for (int i = 0; i < table.Rows.Count; i++)
            {
                var toAdd = new T();
                toAdd.ParseFromRow(table.Rows[i]);

                ret.Add(toAdd);
            }

            return ret;
        }

        public static DataTable LoadDataTable(string tableName, string where, string order)
        {
            var ds = DBProvider.ExecuteQuery(string.Format("select * from {0} where {1} order by {2}", tableName, where ?? "1 = 1", order ?? "ID desc"));
            if (ds == null || ds.Tables == null || ds.Tables.Count == 0)
                return new DataTable();

            return ds.Tables[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="where"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public static List<T> Load(string tableName, string where, string order)
        {
            var ret = new List<T>();
            var table = LoadDataTable(tableName, where, order);

            for (int i = 0; i < table.Rows.Count; i++)
            {
                var toAdd = new T();
                toAdd.ParseFromRow(table.Rows[i]);

                ret.Add(toAdd);
            }

            return ret;
        }

        /// <summary>
        /// returns datarow with entity with given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DataRow GetById(long id)
        {
            var ds = DBProvider.ExecuteQuery(string.Format("select * from {0} where id = {1}", GetTableName(), id));
            if (ds == null || ds.Tables == null || ds.Tables.Count == 0 || ds.Tables[0].Rows == null || ds.Tables[0].Rows.Count == 0)
                return null;

            return ds.Tables[0].Rows[0];
        }

        /// <summary>
        /// Nacita a vyparsuje entitu podla skutocneho typu
        /// </summary>
        /// <param name="id"></param>
        public virtual void Load(long id)
        {
            Clear();

            if (id < 0)
                return;

            var row = GetById(id);
            if (row == null)
                return;

            ParseFromRow(row);
        }

        /// <summary>
        /// Save the changes or insert new entity when Id is null
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="values"></param>
        public void Save(string columns, string values)
        {
            DBProvider.ExecuteNonQuery(string.Format("insert or replace into {0} ( {1} ) values ( {2} )",
                GetTableName(),
                columns,
                values
                ));
        }

        /// <summary>
        /// Updates entity with given ID
        /// </summary>
        /// <param name="what"></param>
        public virtual void Update(string what)
        {
            DBProvider.ExecuteNonQuery(string.Format("update {0} set {1} where {2} = {3}",
                GetTableName(),
                what,
                ID, Id
                ));
        }

        /// <summary>
        /// Deletes/invalidates the entity
        /// </summary>
        public virtual void Delete()
        {
            Valid = false;
            Update(string.Format("{0} = {1}", VALID, 0));
        }

        /// <summary>
        /// Parse base properties from data row
        /// </summary>
        /// <param name="row"></param>
        internal virtual void ParseFromRow(System.Data.DataRow row)
        {
            Id = long.Parse(row[ID].ToString());
            Comment = row[COMMENT].ToString();
            Valid = row[VALID].ToString() != "0";
        }

        #region IEntity Members

        public abstract string GetTableName();

        #endregion
    }
}
