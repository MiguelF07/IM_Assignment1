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
        private IWebDriver driver;

        public MainWindow()
        {
            InitializeComponent();
            driver = new ChromeDriver("../."); //Uses a specific driver for chrome version 107
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
            Console.WriteLine(((string)json.recognized[0].ToString()));

            switch ((string)json.recognized[0].ToString())
            {
                
             //Opções de Jogo
                case "START":
                    if(driver.FindElements(By.Id("btnNewGame")).Count() > 0)
                    {
                        driver.FindElement(By.Id("btnNewGame")).Click();
                        call_tts("O jogo está a ser iniciado.");
                    }
                    break;
                case "END":
                    if(driver.FindElements(By.Id("help-button")).Count()>0)
                    {
                        driver.FindElement(By.Id("help-button")).Click();
                        call_tts("O jogo foi terminado.");
                    }
                    break;
                case "RESTART":
                    if (driver.FindElements(By.Id("fold-button")).Count()>0) 
                    {
                        driver.FindElement(By.Id("fold-button")).Click();
                        call_tts("A reiniciar o jogo.");
                    }
                    break;
                case "CONTINUE":
                    if (driver.FindElements(By.Id("call-button")).Count()>0)
                    {
                        driver.FindElement(By.Id("call-button")).Click();
                        call_tts("Continuar Jogo");
                    } 
                    break;
                case "PLAYERNAME":
                    //escrever no botão id="name-button"
                    if(driver.FindElements(By.Id("PlayerName")).Count()>0)
                    {
                        call_tts("Que nome de utilizador gostaria de usar?");
                        driver.FindElement(By.Id("PlayerName")).Click();
                        driver.FindElement(By.Id("PlayerName")).SendKeys("Player1");
                        call_tts("O nome de utilizador é Player1"); //Change it to variable later
                        //Figure out how to get input of user
                    }
                    break;

             //Ações do Jogo in Game
                case "CHECK":
                    if(driver.FindElements(By.Id("call-button")).Count()>0)
                    {
                        driver.FindElement(By.Id("call-button")).Click();

                    }
                    break;

                case "CALL":
                    if(driver.FindElements(By.Id("call-button")).Count()>0) 
                    {
                        driver.FindElement(By.Id("call-button")).Click();
                    }
                    
                    break;
                case "RAISE":
                    if(driver.FindElements(By.Id("raise-button")).Count() > 0)
                    {
                        driver.FindElement(By.Id("raise-button")).Click();//confirmar se funciona e adicionar valor
                    }
                    break;

                case "FOLD":
                    if(driver.FindElements(By.Id("fold-button")).Count()>0)
                    {
                        driver.FindElement(By.Id("fold-button")).Click();
                    }
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

        }

        private void call_tts(String speech)
        {
            //  new 16 april 2020
            mmic.Send(lce.NewContextRequest());

            string json2 = ""; // "{ \"synthesize\": [";
            json2 += speech;
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
