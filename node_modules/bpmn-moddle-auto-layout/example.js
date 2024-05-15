var AutoLayout = require('./index');

var xmlWithoutDi = ' <?xml version="1.0" encoding="UTF-8"?>'+
'<bpmn:definitions xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" xmlns:di="http://www.omg.org/spec/DD/20100524/DI" xmlns:dc="http://www.omg.org/spec/DD/20100524/DC" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" id="Definitions_1" targetNamespace="http://bpmn.io/schema/bpmn" exporter="Camunda Modeler" exporterVersion="0.7.0-dev">'+
  '<bpmn:process id="Process_1" isExecutable="false">'+
  '                <bpmn:startEvent id="start">                </bpmn:startEvent>                                 <bpmn:task id="task_bcd2b3a3-a0d2-4087-9998-3911013c6b4f" name ="Test Activity 2">                </bpmn:task>                                <bpmn:sequenceFlow id="task_49a82506-3991-4da1-8507-c669115e57e1_to_task_bcd2b3a3-a0d2-4087-9998-3911013c6b4f" sourceRef="task_49a82506-3991-4da1-8507-c669115e57e1" targetRef="task_bcd2b3a3-a0d2-4087-9998-3911013c6b4f" />                                             <bpmn:task id="task_49a82506-3991-4da1-8507-c669115e57e1" name ="Test Activity 1">                </bpmn:task>                                <bpmn:sequenceFlow id="start_to_task_49a82506-3991-4da1-8507-c669115e57e1" sourceRef="start" targetRef="task_49a82506-3991-4da1-8507-c669115e57e1" />                            ' +
  '</bpmn:process>'+
'</bpmn:definitions>';

var autoLayout = new AutoLayout();



autoLayout.layoutProcess(xmlWithoutDi, function(err,test){
    console.log(test)
  
});
