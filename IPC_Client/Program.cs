using IPC_Lib;
using System.IO.Pipes;

public class IpcClient
{
    public static void Main(string[] args)
    {

        var pipeClient =
            new NamedPipeClientStream(".", "comm_pipe", PipeDirection.InOut);

        Console.WriteLine("Свързване към сървър...\n");
        pipeClient.Connect();

        var ss = new StringStream(pipeClient);
        // Валидираме, че сървъра е готов за работа
        if (ss.ReadString() == "server connected\n")
        {
            string message;
            do
            {
                Console.Write("Въведете съобщение: ");
                message = Console.ReadLine();
                //изпращаме съобщението към сървъра
                ss.WriteString(message);

                //прочитаме отговора от сървъра
                Console.Write(ss.ReadString());

            } while (message != "exit");
        }
        else
        {
            //ако при свързването съобщението от сървъра не е очакваното ...
            Console.WriteLine("Сървърът не може да бъде верифициран.");
        }

        Console.WriteLine(ss.ReadString());

        Thread.Sleep(4000);
    }
}


