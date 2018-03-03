
using System;
using System.IO.Ports;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace ArduinoCommunicator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Application a = new Application();
            a.App();
        }
    }

    class Application
    {
        MySqlConnection connect;

        public void App()
        {
            String serialPort = ConfigurationManager.AppSettings["serialPort"];
            Console.WriteLine(serialPort);

            SerialPort myPort = new SerialPort();
            myPort.DataBits = 8;
            myPort.BaudRate = 115200;
            myPort.DtrEnable = true;
            myPort.PortName = serialPort;
            myPort.Parity = Parity.None;
            myPort.StopBits = StopBits.One;
            myPort.DataReceived += MyPortDataReceived;

            connect = new MySqlConnection
                (
                "user id=root;" +
                "server=localhost;" +
                "port=3306;" +
                "database=monitor; " +
                "connection timeout=30"
                );

            try
            {
                connect.Open();
                myPort.Open();

                Console.WriteLine("press return to quit");
                Console.ReadLine();

                myPort.Close();
                connect.Close();

                /*                
                                string sql = "INSERT INTO data (sensor, pressure, humidity, temperature) VALUES (4, 30.33, 55.55, 66.6)";
                                Console.WriteLine(sql);

                                var cmd = new MySqlCommand(sql, connect);

                                if (cmd == null) Console.WriteLine("new MySqlCommand returned null");

                                int affected = cmd.ExecuteNonQuery();
                                Console.WriteLine("Lines affected= {0}", affected);

                                connect.Close();

                                Console.ReadKey();
                */
            }

            catch (Exception ex)
            {
                Console.WriteLine("caught exception " + ex.Message);
                Console.ReadKey();
                //throw;
            }
        }

        void MyPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Console.Write(System.DateTime.Now.ToString() + "  --  ");
                var myPort = sender as SerialPort;
                string line = myPort.ReadLine();
                line = line.Trim(); //trim off \r delimiter

                if (line.StartsWith("//"))
                {
                    Console.WriteLine(line);
                    return;
                }

                Console.Write(line);
                string[] item;

                item = line.Split(new Char[] { ',' }, 3);

                string sql = "";
                string values;
                values = item[1] + ", " + item[2];

                switch(item[0])
                {
                    case "H":
                        sql = "INSERT INTO data (sensor, humidity) VALUES (" + values + ");";
                        break;

                    case "P":
                        sql = "INSERT INTO data (sensor, pressure) VALUES (" + values + ");";
                        break;

                    case "T":
                        sql = "INSERT INTO data (sensor, temperature) VALUES (" + values + ");";
                        break;

                    default:
                        break;
                }

                if (sql.Length > 0)
                {
                    Console.Write("  --  ");
                    Console.Write(sql);

                    var cmd = new MySqlCommand(sql, connect);

                    if (cmd == null) Console.Write(" -- new MySqlCommand returned null");

                    int affected = cmd.ExecuteNonQuery();
                    //Console.Write("  --  Lines affected= {0}", affected);
                    Console.WriteLine();
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(" @@@@@@@@ caught exception " + ex.Message);
                //Console.ReadKey();
            }
        }

    }
}
