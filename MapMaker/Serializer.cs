using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker
{
    public class Deserializer<V> where V : new()
    {
        public V Run(DatabaseTable table)
        {
            V v = new V();
            v = iterate(table, v);
            return v;
        }

        public U iterate<U>(DatabaseTable table, U u, string prep = "")
        {
            //get type for first path; e.g.: pos
            foreach (var prop in u.GetType().GetProperties())
            {
                string name = prop.Name;
                string type = prop.PropertyType.ToString();
                string objectType = prop.PropertyType.FullName;
                Type otype = Type.GetType(objectType);
                if (type.Substring(0, "System.".Count()).CompareTo("System.") != 0)
                {
                    //create the object
                    object o = Activator.CreateInstance(otype);

                    //iterate over again
                    Type otype2 = Type.GetType(prop.PropertyType.FullName);
                    MethodInfo method = typeof(Deserializer<V>).GetMethod("iterate");
                    MethodInfo genericMethod = method.MakeGenericMethod(otype2);
                    o = genericMethod.Invoke(this, new object[3] { table, o, prep == "" ? name : prep + "." + name });

                    u = setObjectProp2<U>(u, name, o);

                }
                else
                {
                    MethodInfo method = typeof(Deserializer<V>).GetMethod("setObjectProp");
                    MethodInfo genericMethod = method.MakeGenericMethod(u.GetType());

                    foreach (DatabaseRow row in table)
                    {
                        if (row.Key == prep + "." + name) // doesn't work for global :/
                        {
                            //u = (U)genericMethod.Invoke(this, new object[3] { u, "X", 1.ToString() });
                            u = (U)genericMethod.Invoke(this, new object[3] { u, name, row.Value });
                        }
                    }
                }
            }

            return u;
        }

        public T setObjectProp<T>(T o, string key, string val)
        {
            System.Reflection.PropertyInfo prop = typeof(T).GetProperty(key);

            Type t = prop.PropertyType; // t will be System.String

            if (t.FullName == "System.String")
            {
                prop.SetValue(o, val, null);
            }
            else if (t.FullName == "System.Boolean")
            {
                prop.SetValue(o, ToBool(val));
            }
            else if (t.FullName == "System.Int32")
            {
                prop.SetValue(o, ToInt32(val), null);
            }
            else
            {
                throw new Exception("Type not found in setObjectProp<T>");
            }

            return o;

        }
        public T setObjectProp2<T>(T o, string key, object val) //2 so that getMethod is not ambigous
        {
            System.Reflection.PropertyInfo prop = typeof(T).GetProperty(key);
            prop.SetValue(o, val, null);
            return o;
        }

        private static int ToInt32(string text)
        {
            int result = 0;
            int.TryParse(text, out result);
            return result;
        }

        private static bool ToBool(string text)
        {
            if (text == "True")
                return true;
            return false;
        }
    }






    public class Serializer<V> where V : new()
    {
        public DatabaseTable Run()
        {
            DatabaseTable databaseTable = new DatabaseTable();
            return createDataGridForObject<V>(databaseTable);
        }
        public DatabaseTable createDataGridForObject<T>(DatabaseTable databaseTable, string prep = "") where T : new()
        {
            T t = new T();

            foreach (var prop in t.GetType().GetProperties())
            {
                string name = prop.Name;
                string type = prop.PropertyType.ToString();

                if (type.Substring(0, "System.".Count()).CompareTo("System.") == 0)
                {
                    type = type.Substring("System.".Count());

                    DatabaseRow databaseRow = new DatabaseRow();

                    if (type == "Int32")
                    {
                        //add int32
                        databaseRow.Key = prep + name;
                        databaseRow.Value = "";
                        databaseTable.Add(databaseRow);
                    }

                    else if (type == "String")
                    {
                        //add string
                        databaseRow.Key = prep + name;
                        databaseRow.Value = "";
                        databaseTable.Add(databaseRow);
                    }
                }
                else //serialize object
                {
                    Type otype = Type.GetType(prop.PropertyType.FullName);
                    MethodInfo method = typeof(Serializer<V>).GetMethod("createDataGridForObject");
                    MethodInfo genericMethod = method.MakeGenericMethod(otype);
                    object jo = genericMethod.Invoke(this, new object[2] { new DatabaseTable(), name + "." });

                    DatabaseTable recursiveDatabaseRowResult = (DatabaseTable)jo;
                    foreach (DatabaseRow row in recursiveDatabaseRowResult)
                    {
                        databaseTable.Add(row);
                    }
                }
            }

            return databaseTable;
        }
    }
}
