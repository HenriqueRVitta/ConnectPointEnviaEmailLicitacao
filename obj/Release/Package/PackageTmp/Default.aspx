<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="EnviaEmailLicitacoes._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <main>
        <section class="row" aria-labelledby="aspnetTitle">
            <h1 id="aspnetTitle">ConnectPoint</h1>
            <p class="lead">Rotina de envio de e-mail's de licitações conforme configurado Área, Classificação e Modalidade.</p>
            <p class="lead">Período de Execução: Todos os dias entre 14:00 e 14:40h</p>
            <p class="lead">Hora Atual:
            <asp:label runat="server" ID="HoraAtualView" style="font-size: 16px; color:red;"></asp:label>
            </p>
        </section>

        <div class="row">
            <section class="col-md-4" aria-labelledby="gettingStartedTitle">
                <h2 id="gettingStartedTitle">Quantidade de Alertas</h2>
                <p><asp:label runat="server" ID="TotalGeralLicitacoes" style="font-size: 24px;"></asp:label></p>
            </section>
            <section class="col-md-4" aria-labelledby="librariesTitle">
                <h2 id="librariesTitle">Quantidade de Email's enviados</h2>
                <p><asp:label runat="server" ID="TotalEnviados" style="font-size: 24px;" text="" ></asp:label></p>
            </section>

            <section class="col-md-4" aria-labelledby="gettingStartedTitle">
                <h2 id="gettingStartedTitl">Lista dos e-mails</h2>
                <p><label type="text" TextMode="MultiLine" runat="server" ID="ListaEmails" style="font-size: 24px;"/></p>
            </section>
        </div>

        <div class="row">
            <section class="col-md-4" aria-labelledby="gettingStartedTitle">
                <p><asp:label runat="server" ID="fimProcesso" style="font-size: 24px; color:red;" text="Fim do processo de envio..." Visible="false"></asp:label></p>
            </section>
        </div>

    </main>

</asp:Content>
