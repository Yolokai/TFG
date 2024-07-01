<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ABDFinal.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <link rel="stylesheet" href="https://unpkg.com/bpmn-js@17.5.0/dist/assets/diagram-js.css" />
    <link rel="stylesheet" href="https://unpkg.com/bpmn-js@17.5.0/dist/assets/bpmn-js.css" />
    <link rel="stylesheet" href="https://unpkg.com/bpmn-js@17.5.0/dist/assets/bpmn-js.css" />
    <link rel="stylesheet" href="https://unpkg.com/bpmn-js@17.5.0/dist/assets/bpmn-font/css/bpmn.css" />
    <script type="module" src="Script/bpmn-modeler.development.js"></script>
    <script type="module" src="Script/bpmn-viewer.development.js"></script>
    <script type="module" src="Script/Layouter.js"></script>
    <script src="Script/joint.min.js"></script>
    <script src="Script/jquery-3.7.1.min.js"></script>
    <script src="Script/semantic.min.js"></script>
    <script src="Script/Funciones.js?t=<%=DateTime.Now.ToString("yyyyMMddHHmmss_fffffff") %>"></script>
    <link href="CSS/semantic.min.css" rel="stylesheet" />
    <link href="#" rel="shortcut icon" />
    <title></title>
    <style>
        body {
            background-color: #f5f5f5;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 0;
            padding: 0;
            display: flex;
            min-height: 100vh;
        }

        #content {
            flex: 1;
            padding: 20px;
        }

        form.ui.form {
            background-color: #fff;
            padding: 20px;
            border-radius: 5px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
            max-width: 100%;
            margin: auto;
        }

        .ui.red.primary.submit.button {
            background-color: #db2828;
            color: #fff;
            margin-top: 10px;
        }

        #excelContainer {
            margin-top: 20px;
        }

        .ui.segment h2.ui.header {
            text-align: center;
        }

        table.ui.striped.table th,
        table.ui.striped.table td {
            border: 1px solid #ddd;
            padding: 10px;
            text-align: left;
        }

        table.ui.striped.table th {
            background-color: #8080809c;
        }

        table.ui.striped.table tbody tr:nth-child(even) {
            background-color: #f9f9f9;
        }

        table.ui.striped.table tbody tr:hover {
            background-color: #f1f1f1;
        }
    </style>

</head>
<body>
    <div id="content">

        <div class="ui segment">
            <h2 class="ui header">Tabla de Datos</h2>
            <!-- Agregar este botón para cambiar la vista -->
            <div class="ui buttons">
                <button id="btnExcel" class="ui button">Excel</button>
                <button id="btnGrafo" class="ui button">Grafo</button>
                <button id="btnTortuga" class="ui button">Tortuga</button>
            </div>
        </div>

        <form class="ui form" enctype="multipart/form-data" style="left: 8px; top: -1px">
            <input type="file" name="facturaPeritacion" id="file" accept="application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" />
            <div class="ui red primary submit button">Guardar</div>
            <div id="excelContainer" runat="server" class="ui grid" style="margin-top: 20px; height: 686px;"></div>
            <div id="grafoContainer" style="display: none;"></div>
            <div id="tortugaContainer" style="display: none;"></div>

        </form>
    </div>
</body>
</html>
