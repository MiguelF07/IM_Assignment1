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
using System.Xml;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;
using OpenQA.Selenium.Support.UI;
using System.Threading;

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
        private Dictionary<String, String> cards;
        private Boolean raise_flag;
        private Boolean all_in_flag;
        private Boolean limit_flag;
        private int bet_limit;
        private int currentCash;

        public MainWindow()
        {
            InitializeComponent();
            driver = new ChromeDriver("../."); //Uses a specific driver for chrome version 107
            driver.Navigate().GoToUrl("https://www.playgreatpoker.com/FreePokerGameStart.html");
            cards = addCards();
            raise_flag = false;
            all_in_flag = false;
            limit_flag = false;
            this.bet_limit = -1;
            this.currentCash = -1;


            mmiC = new MmiCommunication("localhost", 8000, "User1", "GUI");
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
            System.Diagnostics.Debug.WriteLine(e.Message);
            Console.WriteLine(e.Message);
            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);
            System.Diagnostics.Debug.WriteLine(((string)json.recognized[0].ToString()));
            System.Diagnostics.Debug.WriteLine("CONFIANCA");
            System.Diagnostics.Debug.WriteLine(((string)json.confidence[0].ToString()));


            var text_of_switch = (string)json.recognized[0].ToString();

            this.currentCash = get_Current_Cash();
            

            if(float.Parse(((string)json.confidence[0].ToString()))<0.45)
            {
                call_tts("Não percebi, pode repetir?");
            }
            else
            {
                if (raise_flag == true)
                {
                    text_of_switch = "RAISE";
                }

                if (all_in_flag == true)
                {
                    text_of_switch = "ALLIN";
                }

                if (limit_flag == true)
                {
                    text_of_switch = "LIMIT";
                }

                if(raise_flag!=true && all_in_flag!=true && limit_flag!=true)
                {
                    verify_Limit();
                }


                switch (text_of_switch)
                {

                    //Opções de Jogo
                    case "START":
                        if (driver.FindElements(By.Id("btnNewGame")).Count() > 0)
                        {
                            driver.FindElement(By.Id("btnNewGame")).Click();
                            call_tts("O jogo está a ser iniciado.");
                        }
                        break;
                    case "END":
                        if (driver.FindElements(By.Id("help-button")).Count() > 0)
                        {
                            driver.FindElement(By.Id("help-button")).Click();
                            call_tts("O jogo foi terminado.");
                        }
                        break;
                    case "RESTART":
                        if (driver.FindElements(By.Id("fold-button")).Count() > 0)
                        {
                            driver.FindElement(By.Id("fold-button")).Click();
                            call_tts("A reiniciar o jogo.");
                        }
                        if (driver.FindElements(By.XPath("//*[@id=\"modal-box\"]/table/tbody/tr/td/table/tbody/tr/td/div/a")).Count() > 0)
                        {
                            driver.FindElement(By.XPath("//*[@id=\"modal-box\"]/table/tbody/tr/td/table/tbody/tr/td/div/a")).Click();
                        }
                        break;
                    case "CONTINUE":
                        if (driver.FindElements(By.Id("call-button")).Count() > 0)
                        {
                            driver.FindElement(By.Id("call-button")).Click();
                            call_tts("Continuar Jogo");
                        }
                        break;
                    case "PLAYERNAME":
                        //escrever no botão id="name-button"
                        if (driver.FindElements(By.Id("PlayerName")).Count() > 0)
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
                        if (driver.FindElements(By.Id("call-button")).Count() > 0)
                        {
                            driver.FindElement(By.Id("call-button")).Click();

                        }
                        break;

                    case "CALL":
                        if (driver.FindElements(By.Id("call-button")).Count() > 0)
                        {
                            driver.FindElement(By.Id("call-button")).Click();
                        }

                        break;
                    case "RAISE":
                        if (driver.FindElements(By.Id("raise-button")).Count() > 0)
                        {
                            var v_min = driver.FindElement(By.Id("raise-button")).Text.Split('$')[1];
                            var v_max = driver.FindElement(By.XPath("//*[@id=\"seat0\"]/div[2]/div[2]")).Text.Split('$')[1];
                            if (raise_flag == false)
                            {
                                call_tts("O valor minimo de aumento é " + v_min + " e o valor máximo é " + v_max + " quanto pretende aumentar ?");
                            }
                            raise_flag = true;
                            if (raise_flag == true)
                            {

                                if (json.recognized[0].ToString().Contains("NUMBERS"))
                                {
                                    var numero = int.Parse(json.recognized[0].ToString().Split('S')[1]);

                                    if (numero % 10 != 0 || numero < int.Parse(v_min) || numero > int.Parse(v_max))
                                    {
                                        call_tts("O número da aposta tem de ser múltiplo de 10 e estar entre os valores referidos.");
                                    }

                                    else
                                    {
                                        var n_of_clicks = (numero - int.Parse(v_min)) / 10;
                                        for (var i = 0; i < n_of_clicks; i++)
                                        {
                                            if (driver.FindElements(By.XPath("//*[@id=\"quick-raises\"]/table/tbody/tr/td/a[7]")).Count() > 0)
                                            {
                                                driver.FindElement(By.XPath("//*[@id=\"quick-raises\"]/table/tbody/tr/td/a[7]")).Click();
                                            }

                                        }
                                        raise_flag = false;
                                        driver.FindElement(By.XPath("//*[@id=\"raise-button\"]")).Click();
                                        call_tts("Foram apostados " + numero + " dólares.");
                                    }
                                }
                            }
                        }
                        break;

                    case "FOLD":
                        if (driver.FindElements(By.Id("fold-button")).Count() > 0)
                        {
                            driver.FindElement(By.Id("fold-button")).Click();
                        }
                        break;
                    case "ALLIN":
                        if(all_in_flag == true)
                        {
                            String command_all_in = (string)json.recognized[0].ToString();
                            if(command_all_in == "YES")
                            {
                                if(driver.FindElements(By.XPath("//*[@id=\"quick-raises\"]/table/tbody/tr/td")).Count()>0)
                                {
                                    var parent = driver.FindElement(By.XPath("//*[@id=\"quick-raises\"]/table/tbody/tr/td"));
                                    var tamanho = parent.FindElements(By.XPath("./*")).Count();
                                    driver.FindElement(By.XPath("//*[@id=\"quick-raises\"]/table/tbody/tr/td/a[" + (tamanho - 1) + "]")).Click(); //Clica no MAX
                                    driver.FindElement(By.Id("raise-button")).Click();
                                    call_tts("Foi apostada a totalidade do seu saldo.");
                                }
                            }
                            else
                            {
                                call_tts("Aposta cancelada.");
                            }
                            all_in_flag = false;
                            
                        }
                        else
                        {
                            call_tts("Tem a certeza que quer apostar tudo?");
                            all_in_flag = true;
                        }

                        break;


                    //Informação sobre o estado de jogo
                    case "TABLE":
                        //informação das cartas na mesa
                        cardsInTable();
                        break;
                    case "HAND":
                        cardsInHand();
                        break;
                    case "POT":
                        if (driver.FindElements(By.Id("total-pot")).Count() > 0)
                        {
                            String pot_total = driver.FindElement(By.Id("total-pot")).Text;
                            call_tts("O valor total apostado atualmente é " + pot_total.Split('$')[1] + "dólares");
                        }
                        //procurar valor no id="total-pot"
                        break;

                    //Definições acrescentadas
                    case "LIMIT":
                        if (limit_flag == true)
                        {
                            if (json.recognized[0].ToString().Contains("NUMBERS"))
                            {
                                var numero = (json.recognized[0].ToString().Split('S')[1]);
                                call_tts("Limite de " + numero + " dólares definido.");
                                limit_flag = false;
                                this.bet_limit = int.Parse(numero);

                            }
                        }
                        else
                        {
                            call_tts("Qual é o valor que pretende definir como limite?");
                            limit_flag = true;
                        }
                        

                        break;

                    case "CASH":
                        String saldo = get_Current_Cash().ToString();
                        call_tts("O seu saldo atual é de " + saldo + " dólares.");
                        break;
                        

                    default:
                        Console.WriteLine("No option selected.");
                        break;

                    
                }
                this.currentCash = get_Current_Cash();
            }

            

        }

        private int get_Current_Cash()
        {
            if (driver.FindElements(By.XPath("//*[@id=\"seat0\"]/div[2]/div[2]")).Count() > 0)
            {
                return int.Parse(driver.FindElement(By.XPath("//*[@id=\"seat0\"]/div[2]/div[2]")).Text.Split('$')[1]);
            }
            return -1;
        }

        private void verify_Limit()
        {
            if((this.currentCash!=-1) && this.bet_limit!=-1)
            {
                if((500-this.currentCash) > this.bet_limit)
                {
                    call_tts("Atenção! Já atingiu o seu limite de apostas.");
                }
            }
        }

        private void call_tts(String speech)
        {
            //  new 16 april 2020
            mmic.Send(lce.NewContextRequest());

            string json2 = ""; 
            json2 += speech;

            var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, json2);
            mmic.Send(exNot);
        }

        private void cardsInHand()
        {
            var textC1 = driver.FindElement(By.XPath("//*[@id=\"seat0\"]/div[1]/div[1]")).GetAttribute("style");
            var card1 = textC1.Split('/')[2].Split('.')[0].ToString();

            var textC2 = driver.FindElement(By.XPath("//*[@id=\"seat0\"]/div[1]/div[2]")).GetAttribute("style");
            var card2 = textC2.Split('/')[2].Split('.')[0].ToString();

            var textCardC1 = cards[card1];
            var textCardC2 = cards[card2];

            call_tts("A sua mão contem " + textCardC1 + " e " + textCardC2);
        }

        private void cardsInTable()
        {
            List<String> cards_in_table = new List<String>();
            cards_in_table.Add(driver.FindElement(By.Id("flop1")).GetAttribute("style").Split('/')[2].Split('.')[0].ToString());
            cards_in_table.Add(driver.FindElement(By.Id("flop2")).GetAttribute("style").Split('/')[2].Split('.')[0].ToString());
            cards_in_table.Add(driver.FindElement(By.Id("flop3")).GetAttribute("style").Split('/')[2].Split('.')[0].ToString());
            cards_in_table.Add(driver.FindElement(By.Id("turn")).GetAttribute("style").Split('/')[2].Split('.')[0].ToString());
            cards_in_table.Add(driver.FindElement(By.Id("river")).GetAttribute("style").Split('/')[2].Split('.')[0].ToString());
            List<String> final_cards = new List<String>();

            for (int i = 0; i < cards_in_table.Count; i++)
            {
                if (cards.ContainsKey(cards_in_table[i]))
                {
                    final_cards.Add(cards_in_table[i]);
                }
            }

            String cardsString = "";
            for(int i = 0; i < final_cards.Count; i++)
            {
                cardsString = cardsString + ", " + final_cards[i];
            }
            if(cardsString == "")
            {
                call_tts("A mesa está vazia.");
            }
            else
            {
                call_tts("A mesa contém " + cardsString);
            }

           
        }

        private Dictionary<String,String> addCards()
        {
            var cards = new Dictionary<String, String>();
            cards.Add("king_of_hearts", "rei de copas");
            cards.Add("queen_of_hearts", "rainha de copas");
            cards.Add("jack_of_hearts", "valete de copas");
            cards.Add("10_of_hearts", "dez de copas");
            cards.Add("9_of_hearts", "nove de copas");
            cards.Add("8_of_hearts", "oito de copas");
            cards.Add("7_of_hearts", "sete de copas");
            cards.Add("6_of_hearts", "seis de copas");
            cards.Add("5_of_hearts", "cinco de copas");
            cards.Add("4_of_hearts", "quatro de copas");
            cards.Add("3_of_hearts", "três de copas");
            cards.Add("2_of_hearts", "dois de copas");
            cards.Add("ace_of_hearts", "ás de copas");
            cards.Add("king_of_diamonds", "rei de ouros");
            cards.Add("queen_of_diamonds", "rainha de ouros");
            cards.Add("jack_of_diamonds", "valete de ouros");
            cards.Add("10_of_diamonds", "dez de ouros");
            cards.Add("9_of_diamonds", "nove de ouros");
            cards.Add("8_of_diamonds", "oito de ouros");
            cards.Add("7_of_diamonds", "sete de ouros");
            cards.Add("6_of_diamonds", "seis de ouros");
            cards.Add("5_of_diamonds", "cinco de ouros");
            cards.Add("4_of_diamonds", "quatro de ouros");
            cards.Add("3_of_diamonds", "três de ouros");
            cards.Add("2_of_diamonds", "dois de ouros");
            cards.Add("ace_of_diamonds", "ás de ouros");
            cards.Add("king_of_spades", "rei de espadas");
            cards.Add("queen_of_spades", "rainha de espadas");
            cards.Add("jack_of_spades", "valete de espadas");
            cards.Add("10_of_spades", "dez de espadas");
            cards.Add("9_of_spades", "nove de espadas");
            cards.Add("8_of_spades", "oito de espadas");
            cards.Add("7_of_spades", "sete de espadas");
            cards.Add("6_of_spades", "seis de espadas");
            cards.Add("5_of_spades", "cinco de espadas");
            cards.Add("4_of_spades", "quatro de espadas");
            cards.Add("3_of_spades", "três de espadas");
            cards.Add("2_of_spades", "dois de espadas");
            cards.Add("ace_of_spades", "ás de espadas");
            cards.Add("king_of_clubs", "rei de paus");
            cards.Add("queen_of_clubs", "rainha de paus");
            cards.Add("jack_of_clubs", "valete de paus");
            cards.Add("10_of_clubs", "dez de paus");
            cards.Add("9_of_clubs", "nove de paus");
            cards.Add("8_of_clubs", "oito de paus");
            cards.Add("7_of_clubs", "sete de paus");
            cards.Add("6_of_clubs", "seis de paus");
            cards.Add("5_of_clubs", "cinco de paus");
            cards.Add("4_of_clubs", "quatro de paus");
            cards.Add("3_of_clubs", "três de paus");
            cards.Add("2_of_clubs", "dois de paus");
            cards.Add("ace_of_clubs", "ás de paus");

            return cards;
        }
    }
}
