var datosObtenidos = null;

$(function () {
    try {
        $('.ui.form').form();
        $('.ui.form').on('submit', handleOnSubmit);
    }
    catch (e) {
        console.error(e.message);
    }

    // Agregar evento para cambiar la vista al hacer clic en el botón
    // Manejar el evento de clic en el botón 'Excel'
    $('#btnExcel').on('click', function () {
        // Ocultar grafoContainer y tortugaContainer (si están visibles)
        $('#grafoContainer, #tortugaContainer').hide();

        // Mostrar excelContainer
        $('#excelContainer').show();
    });

    // Manejar el evento de clic en el botón 'Grafo'
    $('#btnGrafo').on('click', function () {
        // Ocultar excelContainer y tortugaContainer (si están visibles)
        $('#excelContainer, #tortugaContainer').hide();

        // Mostrar grafoContainer
        $('#grafoContainer').show();

        // Generar el diagrama de grafo si es necesario
        generarDiagramaFlujo(datosObtenidos);
    });

    // Manejar el evento de clic en el botón 'Tortuga'
    $('#btnTortuga').on('click', function () {
        // Ocultar excelContainer y grafoContainer (si están visibles)
        $('#excelContainer, #grafoContainer').hide();

        // Mostrar tortugaContainer
        $('#tortugaContainer').show();

        // Generar el diagrama de tortuga si es necesario
        generarDiagramaTortuga();
    });

    // Lógica para inicializar otros componentes o funcionalidades si es necesario
});

function MostrarDatos(data, vistaTabla) {
    if (vistaTabla) {
        // Assuming data is an object with properties corresponding to table columns
        let html = "<table class='ui striped table'><thead><tr>";

        // Add table headers
        for (let key in data.datosExcell) {
            html += "<th>" + key + "</th>";
        }
        html += "</tr></thead><tbody>";

        // Add table rows
        for (let i = 0; i < data.datosExcell[Object.keys(data.datosExcell)[0]].length; i++) {
            html += "<tr>";
            for (let key in data.datosExcell) {
                html += "<td>" + data.datosExcell[key][i] + "</td>";
            }
            html += "</tr>";
        }

        html += "</tbody></table>";

        // Update the content of the specified container with the generated HTML
        $("#excelContainer").html(html);
        $("#excelContainer").show();
        $("#grafoContainer").hide();
    }
    else {
        $("#excelContainer").hide();
        $("#grafoContainer").show();
        if (data != null && data.Grafo != null) {
            console.log(data);
            generarDiagramaFlujo(data.Grafo);
        }
        else {
            console.error('Datos de grafo no válidos o faltantes.');
        }
    }
};

function handleOnSubmit(e) {
    try {
        e.preventDefault();
        if ($('.ui.form').form('is valid')) {
            let form = document.querySelector('.form');
            let Datos = new FormData(form);
            Datos.append("op", "1");

            $.ajax(
                {
                    type: "POST",
                    url: "Default.aspx",
                    processData: false,
                    contentType: false,
                    data: Datos,
                    success: function (data, txtStatus, jqXHR) {
                        switch (data.Estado) {
                            case "ok":
                                datosObtenidos = data;
                                MostrarDatos(data, true);
                                var blob = new Blob([JSON.stringify(data)], { type: "application/json" });
                                var link = document.createElement('a');
                                link.href = window.URL.createObjectURL(blob);
                                link.download = "data.json";
                                document.body.appendChild(link);
                                link.click();
                                document.body.removeChild(link);
                                break;
                            case "nok":
                                console.log(data.error);
                                break;
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        console.log(errorThrown);
                    }
                });
        }
        else {
            console.error('FORMULARIO INCOMPLETO');
        }
    }
    catch (e) {
        console.error(e.message);
    }
};

// Definición de constantes y variables
function generarDiagramaFlujo(GrafoData) {
    $('#grafoContainer').empty();

    var bpmnXML = construirXMLBPMN(grafoOrdenado);
    const viewer = new BpmnJS({ container: '#grafoContainer' });

    console.log(bpmnXML)
    viewer.importXML(bpmnXML, (err) => {
        if (err) {
            console.error('Error al importar BPMN XML', err);
        } else {
            console.log('BPMN XML importado con éxito');
        }
    });
}

function GrafoCrearCyto(grafoData) {
    container: document.getElementById('grafoContainer')
}

function construirXMLBPMN(grafoData) {
    let bpmnXML = `<?xml version="1.0" encoding="UTF-8"?>
<bpmn:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                  xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL"
                  xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI"
                  xmlns:di="http://www.omg.org/spec/DD/20100524/DI"
                  xmlns:dc="http://www.omg.org/spec/DD/20100524/DC"
                  xsi:schemaLocation="http://www.omg.org/spec/BPMN/20100524/MODEL http://www.omg.org/spec/BPMN/2.0/20100501/BPMN20.xsd
                                      http://www.omg.org/spec/DD/20100524/DC https://www.omg.org/spec/BPMN/20100501/DC.xsd
                                      http://www.omg.org/spec/BPMN/20100524/DI https://www.omg.org/spec/BPMN/20100501/BPMNDI.xsd
                                      http://www.omg.org/spec/DD/20100524/DI https://www.omg.org/spec/BPMN/20100501/DI.xsd"
                  id="Definitions_1"
                  targetNamespace="http://bpmn.io/schema/bpmn">
  <bpmn:process id="Process_1" isExecutable="false">`;

    grafoData.Nodes.forEach(node => {
        bpmnXML += construirNodoBPMN(node);
    });

    grafoData.Edges.forEach(link => {
        bpmnXML += construirEnlaceBPMN(link);
    });

    bpmnXML += `</bpmn:process>
        <bpmndi:BPMNDiagram id="BPMNDiagram_1">
            <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_1">`;

    grafoData.Nodes.forEach(node => {
        bpmnXML += construirBPMNShape(node);
    });

    grafoData.Edges.forEach(link => {
        bpmnXML += construirBPMNEdge(link);
    });

    // Cerrar el plano BPMN y las definiciones
    bpmnXML += `</bpmndi:BPMNPlane>
        </bpmndi:BPMNDiagram>
      </bpmn:definitions>`;

    console.log(bpmnXML);
    return bpmnXML;

}
 
function construirNodoBPMN(nodeData) {
    let bpmnNodeXML = '';
    switch (nodeData.type) {
        case "StartEvent":
            bpmnNodeXML = `<bpmn:startEvent id="${nodeData.id}" name="${nodeData.name}"></bpmn:startEvent>`;
            break;
        case "EndEvent":
            bpmnNodeXML = `<bpmn:endEvent id="${nodeData.id}" name="${nodeData.name}"></bpmn:endEvent>`;
            break;
        case "Task":
            bpmnNodeXML = `<bpmn:task id="a_${nodeData.id}" name="${nodeData.name}"></bpmn:task>`;
            break;
        case "ExclusiveGateway":
            bpmnNodeXML = `<bpmn:exclusiveGateway id="a_${nodeData.id}" name="${nodeData.name}"></bpmn:exclusiveGateway>`;
            break;
        case "ParallelGateway":
            bpmnNodeXML = `<bpmn:parallelGateway id="a_${nodeData.id}" name="${nodeData.name}"></bpmn:parallelGateway>`;
            break;
        default:
            break;
    }

    return bpmnNodeXML + '\n';
}

function construirEnlaceBPMN(linkData) {
    let sourceRef = linkData.source === "inicio" ? linkData.source : `a_${linkData.source}`;
    let targetRef = linkData.target === "fin" ? linkData.target : `a_${linkData.target}`;

    return `<bpmn:sequenceFlow id="${linkData.id}" sourceRef="${sourceRef}" targetRef="${targetRef}"></bpmn:sequenceFlow>\n`;
}
