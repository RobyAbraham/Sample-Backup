using ExpressBase.Common;
using ExpressBase.Data;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressBase.ServiceStack
{
    [Route("/users")]
    public class GetUser : IReturn<UserResponse>
    {
        public int Size { get; set; }
        public int Page { get; set; }
        public string Sort { get; set; }
        public string Sort_dir { get; set; }
        public string Filter { get; set; }
}

    public class UserResponse
    {
        public int Last_Page { get; set; }
        public List<User> Data { get; set; }
    }

    //[Route("/users", "POST")]
    public class User
    {
        public int Id { get; set; }
        public string User_name{ get; set; }
        public string Pwd { get; set; }
        public string Color { get; set; }

        public User() { }
        public User( int id, string name, string pwd, string color)
        {
            Id = id;
            User_name = name;
            Pwd = pwd;
            Color = color;
        }
    }

    [ClientCanSwapTemplates]
    public class UserService:Service
    {
        public object Get(GetUser req)
        {
            //int decValue = int.Parse("A6A6DF", System.Globalization.NumberStyles.HexNumber);
            string sql = string.Empty;
            var e = InitDb(@"D:\imp\abc.conn");
            DatabaseFactory df = new DatabaseFactory(e);
            if (!string.IsNullOrEmpty(req.Filter))
                sql = string.Format("SELECT COUNT(*) FROM eb_users WHERE user_name LIKE '%{4}%'; SELECT * FROM eb_users WHERE user_name LIKE '%{4}%' ORDER BY {0} {1} LIMIT {2} OFFSET {3};", req.Sort, req.Sort_dir, req.Size, ((req.Page - 1) * req.Size), req.Filter);
            else
                //sql = string.Format("SELECT COUNT(*) FROM eb_users; SELECT * FROM eb_users ORDER BY {0} {1} LIMIT {2} OFFSET {3};", req.Sort, req.Sort_dir, req.Size, ((req.Page - 1) * req.Size));
                sql = "SELECT * FROM eb_users";
            //EbDataSet ds = df.ObjectsDatabase.DoQueries( sql);
            //List<User> lu = new List<User>();
            //foreach (EbDataRow dr in ds.Tables[1].Rows)
            //{
            //    User u = new User();
            //    u.Id = Convert.ToInt32(dr[0]);
            //    u.User_name = dr[1].ToString();
            //    u.Pwd = dr[2].ToString();
            //    u.Color = (dr[3] != DBNull.Value) ? Convert.ToInt32(dr[3]).ToString("X") : null;

            //    lu.Add(u);
            //}
            EbDataTable dt = df.ObjectsDatabase.DoQuery(sql);
            List<User> lu = new List<User>();
            foreach (EbDataRow dr in dt.Rows)
            {
                User u = new User();
                u.Id = Convert.ToInt32(dr[0]);
                u.User_name = dr[1].ToString();
                u.Pwd = dr[2].ToString();
                u.Color = (dr[3] != DBNull.Value) ? Convert.ToInt32(dr[3]).ToString("X") : null;

                lu.Add(u);
            }

            return new UserResponse
            {
               // Last_Page = (req.Size > 0) ? (int)Math.Ceiling((decimal)Convert.ToInt32(ds.Tables[0].Rows[0][0]) / req.Size) : 0,
                Data = lu
            };
        }

        //public object Post(GetUser req)
        //{
        //    var e = InitDb(@"D:\imp\abc.conn");
        //    DatabaseFactory df = new DatabaseFactory(e);
        //    string sql = string.Format("SELECT COUNT(*) FROM eb_users; SELECT * FROM eb_users WHERE user_name LIKE %{0}%",req.filter);
        //    EbDataSet  ds = df.ObjectsDatabase.DoQueries(sql);
        //    List<User> lu = new List<User>();
        //    foreach (EbDataRow dr in ds.Tables[1].Rows)
        //    {
        //        User u = new User();
        //        u.Id = Convert.ToInt32(dr[0]);
        //        u.User_name = dr[1].ToString();
        //        u.Pwd = dr[2].ToString();
        //        u.Color = (dr[3] != DBNull.Value) ? Convert.ToInt32(dr[3]).ToString("X") : null;

        //        lu.Add(u);
        //    }

        //    return new UserResponse
        //    {
        //        Last_Page = (req.Size > 0) ? (int)Math.Ceiling((decimal)Convert.ToInt32(ds.Tables[0].Rows[0][0]) / req.Size) : 0,
        //        Data = lu
        //    };
        //}

        private EbConfiguration InitDb(string path)
        {
            EbConfiguration e = new EbConfiguration()
            {
                ClientID = "xyz0007",
                ClientName = "XYZ Enterprises Ltd.",
                LicenseKey = "00288-22558-25558",
            };

            e.DatabaseConfigurations.Add(EbDatabases.EB_OBJECTS, new EbDatabaseConfiguration(EbDatabases.EB_OBJECTS, DatabaseVendors.PGSQL, "eb_objects", "localhost", 5432, "postgres", "infinity", 500));
            e.DatabaseConfigurations.Add(EbDatabases.EB_DATA, new EbDatabaseConfiguration(EbDatabases.EB_DATA, DatabaseVendors.PGSQL, "eb_objects", "localhost", 5432, "postgres", "infinity", 500));
            e.DatabaseConfigurations.Add(EbDatabases.EB_ATTACHMENTS, new EbDatabaseConfiguration(EbDatabases.EB_ATTACHMENTS, DatabaseVendors.PGSQL, "eb_objects", "localhost", 5432, "postgres", "infinity", 500));
            e.DatabaseConfigurations.Add(EbDatabases.EB_LOGS, new EbDatabaseConfiguration(EbDatabases.EB_LOGS, DatabaseVendors.PGSQL, "eb_objects", "localhost", 5432, "postgres", "infinity", 500));

            byte[] bytea = EbSerializers.ProtoBuf_Serialize(e);
            EbFile.Bytea_ToFile(bytea, path);
            System.Threading.Thread.Sleep(3000);
            return ReadTestConfiguration(path);
        }

        public static EbConfiguration ReadTestConfiguration(string path)
        {
            return EbSerializers.ProtoBuf_DeSerialize<EbConfiguration>(EbFile.Bytea_FromFile(path));
        }
    }
}
