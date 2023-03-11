using IPC_Lib;
using System.IO.Pipes;

public class IpcServer
{
    private const int threadCount = 2;

    public static void Main()
    {
        int i;
        Thread[] servers = new Thread[threadCount];

        Console.WriteLine("Пример за клиент-сървър комуникация чрез библиотеката System.IO.Pipes\n");
        Console.WriteLine($"Mаксимален брой клиенти: {threadCount}]\n");

        // стартираме нови нишки, на всяка от които работи нов NamedPipeServerStream
        for (i = 0; i < threadCount; i++)
        {
            servers[i] = new Thread(ServerThread);
            servers[i].Start();
        }
        Thread.Sleep(250);
        // проверяваме през определено време (250ms) дали нишките са приключили работа заради грешки
        // и ако всички са завършили, излизаме от цикъла и от основния процес.
        while (i > 0)
        {
            for (int j = 0; j < threadCount; j++)
            {
                if (servers[j] != null)
                {
                    if (servers[j].Join(250))
                    {
                        Console.WriteLine("Сървърна нишка [{0}] прекъксна работа.", servers[j].ManagedThreadId);

                        servers[j] = null;
                        i--;
                        Console.WriteLine($"Брой на активните нишки: {i}");
                    }
                }
            }
        }
        Console.WriteLine("\nВсички нишки завършиха изпълнение.");
    }

    private static void ServerThread(object data)
    {
        NamedPipeServerStream pipeServer =
            new NamedPipeServerStream("comm_pipe", PipeDirection.InOut, threadCount);

        int threadId = Thread.CurrentThread.ManagedThreadId;

        // Нишката слуша за връзка с нов клиент докато главнип процес работи.
        // При прекъсване на връзката по някаква причина, външния цикъл запозва отначало и
        // изчаква свързване с нов клиент.
        while (true)
        {
            try
            {
                Console.WriteLine($"Изчакване на клиент на нишка: [{threadId}]\n");
                pipeServer.WaitForConnection();
                Console.WriteLine($"Свързан клиент на нишка: [{threadId}].");

                StringStream ss = new StringStream(pipeServer);

                ss.WriteString("server connected\n");
                string request = String.Empty;

                while (request != "exit")
                {
                    request = ss.ReadString();

                    //Тест за фатална грешка
                    if (request == "error")
                    {
                        throw new Exception("Fatal error test");
                    }

                    Console.WriteLine($"Получено съобщение: [{request}] на нишка [{threadId}] от потребител: {pipeServer.GetImpersonationUserName()}.\n");

                    //За целите на демонстрацията, сървъра връща съвсем прост отговор към клиента
                    ss.WriteString($"Отговор на: {request} - [OK]\n");
                }

                string disconnect = $"Прекъсване на връзката на нишка [{threadId}] с потребител: {pipeServer.GetImpersonationUserName()}";
                ss.WriteString(disconnect);
                Console.WriteLine(disconnect);
                pipeServer.Disconnect();

            }
            // Обработване на грешки при нарушена връзка между клиента и сървъра

            catch (IOException e)
            {
                Console.Write("Грешка: ");
                if (e is EndOfStreamException)
                {
                    Console.Write("Връзката с клиента не може да бъде осъществена.\n");
                    Console.WriteLine(e.Message);

                }
                else
                {
                    Console.Write($"{e.Message}\n");
                }


                pipeServer.Disconnect();
            }
            catch (Exception e)
            {
                pipeServer.Close();
                Console.WriteLine($"Фатална грешка: {e.Message}");
                break;
            }


        }
    }
}


