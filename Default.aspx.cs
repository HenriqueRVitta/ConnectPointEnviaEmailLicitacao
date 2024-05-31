using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Azure;
using Azure.Communication.Email;
using System.Text.RegularExpressions;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using static Mysqlx.Expect.Open.Types.Condition.Types;
using Google.Protobuf;
using System.Data.SqlTypes;
using System.Globalization;
using MySqlX.XDevAPI;
using static System.Windows.Forms.LinkLabel;
using MySqlX.XDevAPI.Common;
using System.Diagnostics;

namespace EnviaEmailLicitacoes
{
    public partial class _Default : Page
    {
        MySqlConnection conexao_1 = new MySqlConnection(String.Format(ConfigurationManager.AppSettings["StrDBConnectPoint"]));
        MySqlConnection conexao_2 = new MySqlConnection(String.Format(ConfigurationManager.AppSettings["StrDBConnectPoint"]));
        MySqlConnection conexao_3 = new MySqlConnection(String.Format(ConfigurationManager.AppSettings["StrDBConnectPoint"]));

        int nTotalEnviados = 0;
        string ListaDeEmailsEviados = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            
            string HoraAtual_ = DateTime.Now.ToString("HH") + DateTime.Now.ToString("mm");
            int HoraAtual = Int32.Parse(HoraAtual_);

            bool executa = false;

            // EXECUTA SEMPRE ENTRE 14:00h e 14:30h
            if (HoraAtual >= 1400 && HoraAtual <= 1440)
                executa = true;

            /*
            if (dataHoraAtual >= dataHoraInicio && dataHoraAtual <= dataHoraFim)
                executa = true;
            */

            // Aborta o processamento se não estiver entre as 14:00h e 14:30h
            if (!executa)
                return;


            TotalGeralLicitacoes.Text = "";
                TotalEnviados.Text = "";
                ListaEmails.InnerText = "";
                fimProcesso.Visible = false;

                conexao_1.Open();

                string sql = "SELECT * FROM `tb_alerta_licitacao` a INNER JOIN tb_usuario u ON a.ID_Usuario = u.Id WHERE Desativado  = 0 and ISNULL(u.DataExclusao)";
                MySqlCommand qrySelect = new MySqlCommand(sql, conexao_1);
                MySqlDataReader readerAlertas = qrySelect.ExecuteReader();

                int nTotalAlertas = 0;

                /* Objeto List para evitar repetição de envio de email */
                List<string> arrayClass = new List<string>();

                /* Objeto List para evitar repetição de envio de email */
                List<string> listLicitacao = new List<string>();

                while (readerAlertas.Read())
                {
                    int lnIDAlertas = (int)readerAlertas["ID"];

                    nTotalAlertas++;

                    string IDEdital = "";

                    string lcWhere = "";
                    string Email = readerAlertas["Email"].ToString();

                    //MODALIDADE
                    string modalidade = readerAlertas["Modalidades"].ToString();
                    modalidade = modalidade.Replace("[", "");
                    modalidade = modalidade.Replace("]", "");

                    /** Troca os caracteres especiais da string por " " **/
                    // modalidade = Regex.Replace(modalidade, @"[^\w\.@-]", " ",RegexOptions.None, TimeSpan.FromSeconds(1.5));

                    if (modalidade != "" && modalidade != "21") {
                        //17 SIGNIFICA TODAS AS MODALIDADES
                        lcWhere += " and e.Modalidade IN ("+modalidade+")";
                    }

                    //CIDADES
                    string cidades = readerAlertas["Cidades"].ToString();
                    cidades = cidades.Replace("[", "");
                    cidades = cidades.Replace("]", "");
                    if (cidades != "") {
                        lcWhere += " and e.ID_Cidade IN ("+cidades+")";
                    }

                    //ESTADOS
                    string estados = readerAlertas["Estados"].ToString();
                    estados = estados.Replace("[", "");
                    estados = estados.Replace("]", "");
                    if (estados != "") {
                        lcWhere = " and e.ID_Estado IN ("+estados+")";
                    }

                    //GRANDE AREA
                    string area = readerAlertas["Especialidades"].ToString();
                    area = area.Replace("[", "");
                    area = area.Replace("]", "");
                    if (area != "") {
                        lcWhere += " and e.ID_Classificacao IN ("+area+")";
                    }

                    string filtro = readerAlertas["FiltroComplementar"].ToString();

                    //FILTRO COMPLEMENTAR
                    if (filtro.ToString() != "") {
                        string json = filtro.ToString();
                        string pattern = @"(?i)[^0-9a-záéíóúàèìòùâêîôûãõç\\s]";
                        //json = Regex.Replace(json, @"[^\w\.@-]","", RegexOptions.None, TimeSpan.FromSeconds(1.5));
                        json = Regex.Replace(json, pattern, " ", RegexOptions.None, TimeSpan.FromSeconds(1.5));
                        lcWhere += " OR Objeto LIKE '%"+json.Trim()+"%'";
                    }

                    string ontem = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd")+" 00:00:00";
                    //DateTime myDate = DateTime.Parse(ontem);

                    lcWhere = "Where e.DataCadastro > '"+ ontem + "' AND ISNULL(e.DataExclusao) AND NOT ISNULL(e.DataAprovacao)" + lcWhere;

                    var cpath = System.Web.HttpContext.Current.Server.MapPath(@"App_Data\template.html");

                    TextReader tr = new StreamReader(cpath);
                    string body = tr.ReadToEnd();
                    tr.Close();

 
                    conexao_2.Open();
                    string sqlEdital = "SELECT e.ID, e.Nome, e.ID_Classificacao, e.DataCadastro, c.Nome AS municipio, c.Uf FROM tb_edital e INNER JOIN tp_cidades c ON e.ID_Cidade = c.ID";
                    string sqlDB = sqlEdital + " " + lcWhere;

                    var dd = Convert.ToDateTime(ontem);

                    MySqlCommand qrySelectDB = new MySqlCommand(sqlDB, conexao_2);
                    MySqlDataReader readerAlertasDB = qrySelectDB.ExecuteReader();

                    int total = 0;
                    string ClassificacaoNivelTres = "";
                    string licitacoes = "";

                    while (readerAlertasDB.Read())
                    {
                        total++;

                        IDEdital = readerAlertasDB["ID"].ToString();
                        var culture = new CultureInfo("pt-BR", false);
                        string date = Convert.ToString(readerAlertasDB["DataCadastro"]);
                        string ano = date.Substring(6, 4);
                        string mes = date.Substring(3, 2);
                        string dia = date.Substring(0, 2);
                        string DataCadastro = ano.ToString() + "-" + mes.ToString() + "-" + dia.ToString();


                        //string DataCadastro = DateTime.ParseExact(date, "dd/MM/yyyy hh:mm:ss", culture)
                        //                            .ToString("yyyy-MM-dd");

                        //string added = readerAlertasDB["DataCadastro"].ToString();
                        string valorClass = readerAlertasDB["ID_Classificacao"].ToString();
                        string descricaoAlerta = readerAlertasDB["Nome"].ToString();

                        string cStringArray = IDEdital+Email;
                        string[] ifFound = listLicitacao.ToArray();
                        bool exists = false;
                        if (ifFound.Contains(cStringArray))
                        {
                            exists = true;
                        }

                        if (exists)
                            continue;


                        listLicitacao.Add(cStringArray);


                        conexao_3.Open();
                        string lcLink = "https://app.connectpoint.com.br/Licitacao/Detalhes?id="+ readerAlertasDB["ID"].ToString();
                        var sqlEditalClass = "SELECT * FROM tb_classificacao WHERE ID = "+readerAlertasDB["ID_Classificacao"]+" AND Inativo = 0";

                        int nivel1 = 0;
                        int nivel2  = 0;
                        int nivel3 = 0;
                        MySqlCommand qrySelectDBClass = new MySqlCommand(sqlEditalClass, conexao_3);
                        MySqlDataReader readerAlertasDBClass = qrySelectDBClass.ExecuteReader();
                        string Classificacao = "";
                        while (readerAlertasDBClass.Read())
                        {
                            Classificacao = readerAlertasDBClass["Codigo"].ToString();
                        }

                        readerAlertasDBClass.Dispose();
                        conexao_3.Close();
                    
                        if (Classificacao.Substring(0, 2) != "")
                        {
                            conexao_3.Open();
                            string sqlClass = "SELECT * FROM tb_classificacao WHERE Codigo = "+Classificacao.Substring(0, 2)+" AND Inativo = 0";
                            MySqlCommand qrySelectDBClassT = new MySqlCommand(sqlClass, conexao_3);
                            MySqlDataReader readerAlertasDBClassT = qrySelectDBClassT.ExecuteReader();
                            while (readerAlertasDBClassT.Read())
                            {
                                nivel1 = (int)readerAlertasDBClassT["ID"];
                            }

                            readerAlertasDBClassT.Dispose();
                            conexao_3.Close();

                        }
                    
                        if (Classificacao.Substring(0, 4) != "")
                        {
                            conexao_3.Open();
                            string sqlClassNivel2 = "SELECT * FROM tb_classificacao WHERE Codigo = " + Classificacao.Substring(0, 4) + " AND Inativo = 0";
                            MySqlCommand qrySelectDBClassTT = new MySqlCommand(sqlClassNivel2, conexao_3);
                            MySqlDataReader readerAlertasDBClassTT = qrySelectDBClassTT.ExecuteReader();
                            while (readerAlertasDBClassTT.Read())
                            {
                                nivel2 = (int)readerAlertasDBClassTT["ID"];
                            }

                            readerAlertasDBClassTT.Dispose();
                            conexao_3.Close();
                        }

                        conexao_3.Open();
                        string DescricaoClassificacao = "";
                        ClassificacaoNivelTres = readerAlertasDB["ID_Classificacao"].ToString();
                        string sqlClassNivel3 = "SELECT * FROM tb_classificacao WHERE ID = "+readerAlertasDB["ID_Classificacao"].ToString()+" AND Inativo = 0";
                        MySqlCommand qrySelectDBClassTTT = new MySqlCommand(sqlClassNivel3, conexao_3);
                        MySqlDataReader readerAlertasDBClassTTT = qrySelectDBClassTTT.ExecuteReader();
                        while (readerAlertasDBClassTTT.Read())
                        {
                            nivel3 = (int)readerAlertasDBClassTTT["ID"];
                            DescricaoClassificacao = readerAlertasDBClassTTT["Descricao"].ToString().Trim();
                        }

                        readerAlertasDBClassTTT.Dispose();
                        conexao_3.Close();

                        lcLink = "https://app.connectpoint.com.br/Licitacao/Detalhes?id="+IDEdital;
                        if (nivel1 > 0 && nivel2 > 0 && nivel3 > 0)
                        {
                            lcLink = "https://app.connectpoint.com.br/Licitacao/Filtrar?Page=1&TipoLocalidadeAtuacao=1&ComEdital=False&Vigentes=False&Favoritos=False&Classificacao="+nivel1.ToString()+"&ClassificacaoNivelDois="+nivel2.ToString()+"&ClassificacaoNivelTres="+nivel3.ToString()+"&DataCadastro="+DataCadastro;
                        }
                        if(descricaoAlerta.Length == 0)
                        {
                            descricaoAlerta = "Click aqui para acessar... " + IDEdital.ToString();
                        }
                        licitacoes += "<p><b>Classificação:</b>"+DescricaoClassificacao+"</p>"+ "\r\n";
                        licitacoes += "<h1><a href="+lcLink.Trim()+">"+descricaoAlerta.Trim()+"</a></h1>" + "\r\n";
                        licitacoes += "<hr />";

                        // Se nenhuma licitação foi encontrada, coloca esta informação no corpo
                        if (total == 0) { 
                            body = body.Replace("[corpo]", "Nenhuma licitação foi encontrada.");
                        } else
                        {
                            body = body.Replace("[corpo]", licitacoes);
                        }

                        conexao_3.Open();

                        // Checagem se o Email do Edital já foi enviado ao Cliente
                        sql = "Select Email from tb_alertas_enviados Where ID_Edital = "+IDEdital+" AND Email = '"+Email+"'";
                        MySqlCommand qrySelectDBClassP = new MySqlCommand(sql, conexao_3);
                        MySqlDataReader readerAlertasDBClassP = qrySelectDBClassP.ExecuteReader();
                        while (readerAlertasDBClassP.Read())
                        {
                            licitacoes = "";
                        }

                        readerAlertasDBClassP.Dispose();
                        conexao_3.Close();

                        string cStringArrayP = nivel3.ToString()+Email+ readerAlertas["Especialidades"];
                        string[] ifFoundP = arrayClass.ToArray();
                        if (ifFoundP.Contains(cStringArrayP))
                        {
                            licitacoes = "";
                        } else
                        {
                            arrayClass.Add(cStringArrayP);
                        }

                        if(licitacoes.Length > 0)
                        {
                            var email = Email;
                            EnviaEmail(email, body, IDEdital);
                        } else
                        {
                            continue;
                        }

                    }

                    qrySelectDB.Dispose();
                    conexao_2.Close();

                }

                qrySelect.Dispose();
                conexao_1.Close();

                TotalGeralLicitacoes.Text = nTotalAlertas.ToString();
                TotalEnviados.Text = this.nTotalEnviados.ToString();
                ListaEmails.InnerText = this.ListaDeEmailsEviados;
                fimProcesso.Visible = true;

            }


            private void EnviaEmail(string email, string body, string IDEdital)
            {
                //string email_2 = "chrvitta@gmail.com";

                // Este código recupera a cadeia de caracteres de conexão de uma variável de ambiente.
                string connectionString = "endpoint=https://servicosemailconnecpoint.unitedstates.communication.azure.com/;accesskey=neUSM7lDcIgTu1Ce/7tvu5otY2LedTPmeOo8d6w57r6ljCslL0StLX8m/L7sRtN592SgmXggh98+3lQH1QFpzg==";
                var emailClient = new EmailClient(connectionString);
                // senderAddress: "DoNotReply@connectpoint.com.br",

                try
                {

                    EmailSendOperation emailSendOperation = emailClient.Send(
                        WaitUntil.Completed,
                        senderAddress: "naoresponda@connectpoint.com.br",
                        recipientAddress: email,
                        subject: "Avisos de Licitação",
                        htmlContent: body,
                        plainTextContent: body);
                
                        string dataAtual = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                        conexao_3.Open();

                        // Insert na tabela tb_alertas_enviados para evitar envio de emails dulicados dos editais
                        string sql = "Insert Into tb_alertas_enviados (ID_Edital, Email, DataEnvio) values ("+IDEdital.ToString()+",'"+email+"','"+ dataAtual+"')";
                        MySqlCommand qrySelect = new MySqlCommand(sql, conexao_3);
                        MySqlDataReader readerSelect = qrySelect.ExecuteReader();
                        readerSelect.Dispose();
                        conexao_3.Close();
                
                        this.nTotalEnviados++;
                        this.ListaDeEmailsEviados += email+ "\r\n";


                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.ToString());

                }

        }
    }

}