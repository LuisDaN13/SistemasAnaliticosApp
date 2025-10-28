using Humanizer;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.SqlServer.Server;
using SistemasAnaliticos.Entidades;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using static System.Collections.Specialized.BitVector32;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SistemasAnaliticos.Entidades
{
    public class Permiso
    {

    }
}

//CITA MEDICA
//< !DOCTYPE html >
//< html lang = "es" >
//< head >
//    < meta charset = "UTF-8" >
//    < meta name = "viewport" content = "width=device-width, initial-scale=1.0" >
//    < title > Modal de Permisos</title>
//    <!-- Bootstrap CSS -->
//    <link href = "https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel= "stylesheet" >
//    < !--Bootstrap Icons -->
//    <link rel = "stylesheet" href= "https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.0/font/bootstrap-icons.css" >
//    < style >
//        .modal - backdrop {
//            background-color: rgba(0, 0, 0, 0.6);
//backdrop - filter: blur(4px);
//        }
//        .modal - header {
//background: linear - gradient(to right, #012473, #013599);
//            color: white;
//}
//        .btn - primary - custom {
//    background - color: #012473;
//            border - color: #012473;
//        }
//        .btn - primary - custom:hover {
//            background-color: #013599;
//            border - color: #013599;
//        }
//        .section - divider {
//    border - top: 1px solid #dee2e6;
//            padding - top: 1.25rem;
//}
//    </ style >
//</ head >
//< body >
//    < !--Botón para abrir el modal -->
//    <div class= "container mt-5" >
//        < button type = "button" class= "btn btn-primary" data - bs - toggle = "modal" data - bs - target = "#permissionModal" >
//            Abrir Modal de Permisos
//        </button>
//    </div>

//    <!-- Modal -->
//    <div class= "modal fade" id = "permissionModal" tabindex = "-1" aria - labelledby = "permissionModalLabel" aria - hidden = "true" >
//        < div class= "modal-dialog modal-lg" >
//            < div class= "modal-content" >
//                < !--Header-- >
//                < div class= "modal-header" >
//                    < h5 class= "modal-title fw-bold" id = "permissionModalLabel" > Nueva Cita Médica</h5>
//                    <button type = "button" class= "btn-close btn-close-white" data - bs - dismiss = "modal" aria - label = "Close" ></ button >
//                </ div >


//                < !--Form-- >
//                < form id = "permissionForm" >
//                    < div class= "modal-body" >
//                        < div >
//                            < !--Fechas y Días -->
//                            <div class= "row mb-3" >
//                                < div class= "col-md-6 mt-1" >
//                                    < label for= "startDate" class= "form-label fw-semibold" >
//                                        Fecha Inicio < span class= "text-danger" > *</ span >
//                                    </ label >
//                                    < input
//                                        type = "date"
//                                        class= "form-control"
//                                        required >
//                                </ div >
//                                < div class= "col-md-6 mt-1" >
//                                    < label for= "endDate" class= "form-label fw-semibold" >
//                                        Fecha Fin < span class= "text-danger" > *</ span >
//                                    </ label >
//                                    < input
//                                        type = "date"
//                                        class= "form-control"
//                                        required >
//                                </ div >

//                                < div class= "col-md-6 mt-4" >
//                                    < label for= "endTime" class= "form-label fw-semibold" >
//                                        Hora de la Cita
//                                        <span class= "text-danger" > *</ span >
//                                    </ label >
//                                    < input type = "time" class= "form-control" id = "endTime" name = "endTime" required >
//                                </ div >
//                                < div class= "col-md-6 mt-4" >
//                                    < label for= "endDate" class= "form-label fw-semibold" >
//                                        Fecha de Regreso Laboral <span class= "text-danger" > *</ span >
//                                    </ label >
//                                    < input
//                                        type = "date"
//                                        class= "form-control"
//                                        required >
//                                </ div >
//                            </ div >
//                        </ div >


//                        < div class= "section-divider" >
//                            < !--Motivo-- >
//                            < div class= "mb-3" >
//                                < label for= "reason" class= "form-label fw-semibold" >
//                                    Motivo < span class= "text-danger" > *</ span >
//                                </ label >
//                                < textarea
//                                    class= "form-control"
//                                    id = "reason"
//                                    name = "reason"
//                                    rows = "3"
//                                    placeholder = "Describe el motivo de la cita..."
//                                    required ></ textarea >
//                            </ div >


//                            < !--Datos Adjuntos-- >
//                            < div class= "mb-3" >
//                                < label for= "notes" class= "form-label fw-semibold" > Datos Adjuntos </ label >
//                                < input
//                                    type = "file"
//                                    class= "form-control"
//                                    id = "notes"
//                                    name = "notes"
//                                    accept = ".pdf,.doc,.docx,.txt,.jpg,.png" >
//                                < div class= "form-text" > Puede seleccionar uno</div>
//                            </div>
//                        </div>
//                    </div>

//                    <!-- Footer -->
//                    <div class= "modal-footer bg-light" >
//                        < button type = "button" class= "btn btn-outline-secondary" data - bs - dismiss = "modal" > Cancelar </ button >
//                        < button type = "submit" class= "btn btn-primary-custom text-white" >
//                            < i class= "bi bi-save me-2" ></ i > Guardar
//                        </ button >
//                    </ div >
//                </ form >
//            </ div >
//        </ div >
//    </ div >

//    < !--Bootstrap JS-- >
//    < script src = "https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js" ></ script >


//    < !--JavaScript para la funcionalidad -->
//    <script>
//        document.addEventListener('DOMContentLoaded', function() {
//            const form = document.getElementById('permissionForm');
//const startDateInput = document.getElementById('startDate');
//const endDateInput = document.getElementById('endDate');
//const daysInput = document.getElementById('days');

//// Función para calcular días
//function calculateDays()
//{
//    const startDate = new Date(startDateInput.value);
//    const endDate = new Date(endDateInput.value);

//    if (startDate && endDate && startDate <= endDate)
//    {
//        const diffTime = Math.abs(endDate.getTime() - startDate.getTime());
//        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1;
//        daysInput.value = diffDays;
//    }
//    else
//    {
//        daysInput.value = '';
//    }
//}

//// Event listeners para calcular días cuando cambian las fechas
//startDateInput.addEventListener('change', calculateDays);
//endDateInput.addEventListener('change', calculateDays);

//// Manejar el envío del formulario
//form.addEventListener('submit', function(e) {
//    e.preventDefault();

//    // Recopilar datos del formulario
//    const formData = {
//                    employeeId: document.getElementById('employeeId').value,
//                    employeeName: document.getElementById('employeeName').value,
//                    permissionType: document.getElementById('permissionType').value,
//                    startDate: startDateInput.value,
//                    endDate: endDateInput.value,
//                    days: daysInput.value,
//                    reason: document.getElementById('reason').value,
//                    status: document.getElementById('status').value,
//                    notes: document.getElementById('notes').value
//                }
//;

//console.log('Datos del permiso:', formData);

//// Aquí iría la lógica para guardar el permiso
//// ...

//// Cerrar el modal
//const modal = bootstrap.Modal.getInstance(document.getElementById('permissionModal'));
//modal.hide();

//// Resetear el formulario
//form.reset();
//            });
//        });
//    </ script >
//</ body >
//</ html >

//BENEFICIO
//< !DOCTYPE html >
//< html lang = "es" >
//< head >
//    < meta charset = "UTF-8" >
//    < meta name = "viewport" content = "width=device-width, initial-scale=1.0" >
//    < title > Modal de Permisos</title>
//    <!-- Bootstrap CSS -->
//    <link href = "https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel= "stylesheet" >
//    < !--Bootstrap Icons -->
//    <link rel = "stylesheet" href= "https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.0/font/bootstrap-icons.css" >
//    < style >
//        .modal - backdrop {
//            background-color: rgba(0, 0, 0, 0.6);
//backdrop - filter: blur(4px);
//        }
//        .modal - header {
//background: linear - gradient(to right, #012473, #013599);
//            color: white;
//}
//        .btn - primary - custom {
//    background - color: #012473;
//            border - color: #012473;
//        }
//        .btn - primary - custom:hover {
//            background-color: #013599;
//            border - color: #013599;
//        }
//        .section - divider {
//    border - top: 1px solid #dee2e6;
//            padding - top: 1.25rem;
//}
//    </ style >
//</ head >
//< body >
//    < !--Botón para abrir el modal -->
//    <div class= "container mt-5" >
//        < button type = "button" class= "btn btn-primary" data - bs - toggle = "modal" data - bs - target = "#permissionModal" >
//            Abrir Modal de Permisos
//        </button>
//    </div>

//    <!-- Modal -->
//    <div class= "modal fade" id = "permissionModal" tabindex = "-1" aria - labelledby = "permissionModalLabel" aria - hidden = "true" >
//        < div class= "modal-dialog modal-lg" >
//            < div class= "modal-content" >
//                < !--Header-- >
//                < div class= "modal-header" >
//                    < h5 class= "modal-title fw-bold" id = "permissionModalLabel" > Nueva Solicitud de Beneficio</h5>
//                    <button type="button" class= "btn-close btn-close-white" data - bs - dismiss = "modal" aria - label = "Close" ></ button >
//                </ div >


//                < !--Form-- >
//                < form id = "permissionForm" >
//                    < div class= "modal-body" >
//                        < div >
//                            < !--Motivo-- >
//                            < div class= "mb-3" >
//                                < label for= "reason" class= "form-label fw-semibold" >
//                                    Monto de la Solicitud de Beneficio <span class= "text-danger" > *</ span >
//                                </ label >
//                                < input type = "number"
//                                       class= "form-control"
//                                       placeholder = "Ingrese el monto deseado"
//                                       required >
//                            </ div >


//                            < !--Notas-- >
//                            < div class= "mb-3" >
//                                < label for= "notes" class= "form-label fw-semibold" > Comentarios </ label >
//                                < textarea
//                                    class= "form-control"
//                                    rows = "2"
//                                    placeholder = "Información adicional..." ></ textarea >
//                            </ div >
//                        </ div >
//                    </ div >


//                    < !--Footer-- >
//                    < div class= "modal-footer bg-light" >
//                        < button type = "button" class= "btn btn-outline-secondary" data - bs - dismiss = "modal" > Cancelar </ button >
//                        < button type = "submit" class= "btn btn-primary-custom text-white" >
//                            < i class= "bi bi-save me-2" ></ i > Guardar
//                        </ button >
//                    </ div >
//                </ form >
//            </ div >
//        </ div >
//    </ div >

//    < !--Bootstrap JS-- >
//    < script src = "https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js" ></ script >


//    < !--JavaScript para la funcionalidad -->
//    <script>
//        document.addEventListener('DOMContentLoaded', function() {
//            const form = document.getElementById('permissionForm');
//const startDateInput = document.getElementById('startDate');
//const endDateInput = document.getElementById('endDate');
//const daysInput = document.getElementById('days');

//// Función para calcular días
//function calculateDays()
//{
//    const startDate = new Date(startDateInput.value);
//    const endDate = new Date(endDateInput.value);

//    if (startDate && endDate && startDate <= endDate)
//    {
//        const diffTime = Math.abs(endDate.getTime() - startDate.getTime());
//        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1;
//        daysInput.value = diffDays;
//    }
//    else
//    {
//        daysInput.value = '';
//    }
//}

//// Event listeners para calcular días cuando cambian las fechas
//startDateInput.addEventListener('change', calculateDays);
//endDateInput.addEventListener('change', calculateDays);

//// Manejar el envío del formulario
//form.addEventListener('submit', function(e) {
//    e.preventDefault();

//    // Recopilar datos del formulario
//    const formData = {
//                    employeeId: document.getElementById('employeeId').value,
//                    employeeName: document.getElementById('employeeName').value,
//                    permissionType: document.getElementById('permissionType').value,
//                    startDate: startDateInput.value,
//                    endDate: endDateInput.value,
//                    days: daysInput.value,
//                    reason: document.getElementById('reason').value,
//                    status: document.getElementById('status').value,
//                    notes: document.getElementById('notes').value
//                }
//;

//console.log('Datos del permiso:', formData);

//// Aquí iría la lógica para guardar el permiso
//// ...

//// Cerrar el modal
//const modal = bootstrap.Modal.getInstance(document.getElementById('permissionModal'));
//modal.hide();

//// Resetear el formulario
//form.reset();
//            });
//        });
//    </ script >
//</ body >
//</ html >


// CONSTANCIAS
//< !DOCTYPE html >
//< html lang = "es" >
//< head >
//    < meta charset = "UTF-8" >
//    < meta name = "viewport" content = "width=device-width, initial-scale=1.0" >
//    < title > Modal de Permisos</title>
//    <!-- Bootstrap CSS -->
//    <link href = "https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel= "stylesheet" >
//    < !--Bootstrap Icons -->
//    <link rel = "stylesheet" href= "https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.0/font/bootstrap-icons.css" >
//    < style >
//        .modal - backdrop {
//            background-color: rgba(0, 0, 0, 0.6);
//backdrop - filter: blur(4px);
//        }
//        .modal - header {
//background: linear - gradient(to right, #012473, #013599);
//            color: white;
//}
//        .btn - primary - custom {
//    background - color: #012473;
//            border - color: #012473;
//        }
//        .btn - primary - custom:hover {
//            background-color: #013599;
//            border - color: #013599;
//        }
//        .section - divider {
//    border - top: 1px solid #dee2e6;
//            padding - top: 1.25rem;
//}
//    </ style >
//</ head >
//< body >
//    < !--Botón para abrir el modal -->
//    <div class= "container mt-5" >
//        < button type = "button" class= "btn btn-primary" data - bs - toggle = "modal" data - bs - target = "#permissionModal" >
//            Abrir Modal de Permisos
//        </button>
//    </div>

//    <!-- Modal -->
//    <div class= "modal fade" id = "permissionModal" tabindex = "-1" aria - labelledby = "permissionModalLabel" aria - hidden = "true" >
//        < div class= "modal-dialog modal-lg" >
//            < div class= "modal-content" >
//                < !--Header-- >
//                < div class= "modal-header" >
//                    < h5 class= "modal-title fw-bold" id = "permissionModalLabel" > Nueva Constancia </ h5 >
//                    < button type = "button" class= "btn-close btn-close-white" data - bs - dismiss = "modal" aria - label = "Close" ></ button >
//                </ div >


//                < !--Form-- >
//                < form id = "permissionForm" >
//                    < div class= "modal-body" >
//                        < div >
//                            < !--Motivo-- >
//                            < div class= "mb-3" >
//                                < label for= "reason" class= "form-label fw-semibold" >
//                                    Persona o Empresa a quien va dirijida <span class= "text-danger" > *</ span >
//                                </ label >
//                                < textarea
//                                    class= "form-control"
//                                    rows = "2"
//                                    placeholder = "Ingresa el receptor de la constancia..."
//                                    required ></ textarea >
//                            </ div >


//                            < !--Notas-- >
//                            < div class= "mb-3" >
//                                < label for= "notes" class= "form-label fw-semibold" > Comentarios </ label >
//                                < textarea
//                                    class= "form-control"
//                                    rows = "2"
//                                    placeholder = "Información adicional..." ></ textarea >
//                            </ div >
//                        </ div >
//                    </ div >


//                    < !--Footer-- >
//                    < div class= "modal-footer bg-light" >
//                        < button type = "button" class= "btn btn-outline-secondary" data - bs - dismiss = "modal" > Cancelar </ button >
//                        < button type = "submit" class= "btn btn-primary-custom text-white" >
//                            < i class= "bi bi-save me-2" ></ i > Guardar
//                        </ button >
//                    </ div >
//                </ form >
//            </ div >
//        </ div >
//    </ div >

//    < !--Bootstrap JS-- >
//    < script src = "https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js" ></ script >


//    < !--JavaScript para la funcionalidad -->
//    <script>
//        document.addEventListener('DOMContentLoaded', function() {
//            const form = document.getElementById('permissionForm');
//const startDateInput = document.getElementById('startDate');
//const endDateInput = document.getElementById('endDate');
//const daysInput = document.getElementById('days');

//// Función para calcular días
//function calculateDays()
//{
//    const startDate = new Date(startDateInput.value);
//    const endDate = new Date(endDateInput.value);

//    if (startDate && endDate && startDate <= endDate)
//    {
//        const diffTime = Math.abs(endDate.getTime() - startDate.getTime());
//        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1;
//        daysInput.value = diffDays;
//    }
//    else
//    {
//        daysInput.value = '';
//    }
//}

//// Event listeners para calcular días cuando cambian las fechas
//startDateInput.addEventListener('change', calculateDays);
//endDateInput.addEventListener('change', calculateDays);

//// Manejar el envío del formulario
//form.addEventListener('submit', function(e) {
//    e.preventDefault();

//    // Recopilar datos del formulario
//    const formData = {
//                    employeeId: document.getElementById('employeeId').value,
//                    employeeName: document.getElementById('employeeName').value,
//                    permissionType: document.getElementById('permissionType').value,
//                    startDate: startDateInput.value,
//                    endDate: endDateInput.value,
//                    days: daysInput.value,
//                    reason: document.getElementById('reason').value,
//                    status: document.getElementById('status').value,
//                    notes: document.getElementById('notes').value
//                }
//;

//console.log('Datos del permiso:', formData);

//// Aquí iría la lógica para guardar el permiso
//// ...

//// Cerrar el modal
//const modal = bootstrap.Modal.getInstance(document.getElementById('permissionModal'));
//modal.hide();

//// Resetear el formulario
//form.reset();
//            });
//        });
//    </ script >
//</ body >
//</ html >