using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using mmisharp;
using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Windows.Controls;
using OpenQA.Selenium.Interactions;

namespace AppGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MmiCommunication mmiC;

        //  new 16 april 2020
        private MmiCommunication mmiSender;
        private LifeCycleEvents lce;
        private MmiCommunication mmic;
        private ChromeDriver driver;

        public MainWindow()
        {
            InitializeComponent();
            IWebDriver driver = new ChromeDriver("C:\\Users\\MiguelFerreira\\Desktop\\IM_Assignment1\\AppGui\\AppGui\\");
            driver.Navigate().GoToUrl("https://www.playgreatpoker.com/FreePokerGameStart.html");


            mmiC = new MmiCommunication("localhost",8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;
            mmiC.Start();

            // NEW 16 april 2020
            //init LifeCycleEvents..
            lce = new LifeCycleEvents("APP", "TTS", "User1", "na", "command"); // LifeCycleEvents(string source, string target, string id, string medium, string mode
            // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)
            mmic = new MmiCommunication("localhost", 8000, "User1", "GUI");
            

        }

        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            Console.WriteLine(e.Message);
            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);

            switch ((string)json.recognized[0].ToString())
            {
             //Opções de Jogo
                case "START":
                    driver.FindElement(By.Id("btnNewGame")).Click();
                    break;
                case "END":
                    driver.FindElement(By.Id("help-button")).Click();
                    break;
                case "RESTART":
                    driver.FindElement(By.Id("fold-button")).Click();
                    break;
                case "CONTINUE":
                    driver.FindElement(By.Id("call-button")).Click();
                    break;
                case "PLAYERNAME":
                    //escrever no botão id="name-button"
                    break;

             //Ações do Jogo in Game
                case "CHECK":
                    driver.FindElement(By.Id("call-button")).Click();
                    break;
                case "CALL":
                    driver.FindElement(By.Id("call-button")).Click();
                    break;
                case "RAISE":
                    driver.FindElement(By.Id("raise-button")).Click();//confirmar se funciona e adicionar valor
                    break;
                case "FOLD":
                    driver.FindElement(By.Id("fold-button")).Click();
                    break;
                case "ALLIN":
                    
                    break;

             //Informação sobre o estado de jogo
                case "TABLE":
                    //informação das cartas na mesa
                    break;
                case "HAND":
                    //informação das cartas na mão
                    break;
                case "POT":
                    //procurar valor no id="total-pot"
                    break;

             //Definições acrescentadas
                case "LIMIT":

                    break;
            }



            //  new 16 april 2020
            mmic.Send(lce.NewContextRequest());

            string json2 = ""; // "{ \"synthesize\": [";
            json2 += (string)json.recognized[0].ToString()+ " ";
            json2 += (string)json.recognized[1].ToString() + " DONE." ;
            //json2 += "] }";
            /*
             foreach (var resultSemantic in e.Result.Semantics)
            {
                json += "\"" + resultSemantic.Value.Value + "\", ";
            }
            json = json.Substring(0, json.Length - 2);
            json += "] }";
            */
            var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, json2);
            mmic.Send(exNot);


        }
    }
}
