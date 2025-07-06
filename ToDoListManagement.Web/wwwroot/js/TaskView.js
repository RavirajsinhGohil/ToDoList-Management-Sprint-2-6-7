const today = new Date().toISOString().split('T')[0];
var startDateValidation = "";
var endDateValidation = "";
var startDateValidationEdit = "";
var endDateValidationEdit = "";
// $(document).ready(function () {
//     initSortable();

// if (selectedProjectId) {
//     $("#sprintDropDown").prop("disabled", false);
//     $("#addSprintButton").prop("disabled", false);
//     $("#addTaskButton").prop("disabled", false);
// }

// if (selectedSprintId) {
//     $("#sprintDropDown").prop("disabled", false);
//     loadTeamMembers(selectedProjectId);
//     loadSprints(selectedProjectId);
//     GetTasksBySprintId(selectedSprintId);
// }

// $("#projectDropDown").change(function () {
//     selectedProjectId = $(this).val();
//     selectedSprintId = null;
//     if (selectedProjectId) {
//         $("#addSprintButton").prop("disabled", false);
//         loadTeamMembers(selectedProjectId);
//         loadSprints(selectedProjectId);
//         loadBacklogTasks(selectedProjectId);
//         GetTasksBySprintId(selectedSprintId);
//         $("#sprintDropDown").prop("disabled", false);
//         $("#addTaskButton").prop("disabled", false);
//     } else {
//         $(".card-body").empty();
//         $("#backlogContainer").empty();
//         $("#assignedToMeCheck").prop("disabled", true);
//         $("#addTaskButton").prop("disabled", true);
//         $("#teamMembersDropDown").prop("disabled", true);
//         $("#sprintDropDown").prop("disabled", true);
//         $("#addSprintButton").prop("disabled", true);
//         $("#addTaskButton").prop("disabled", true);
//         $("#sprintDropDown").val("");
//         $("#teamMembersDropDown").val("");
//         selectedSprintId = null;
//     }
// });

// $("#addTaskModal").on('hidden.bs.modal', function () {
//     $("#addTaskForm").trigger("reset");
//     $("#addTaskForm").find(".text-danger").text("");
// });

// $("#addSprintModal").on('hidden.bs.modal', function () {
//     $("#addSprintForm").trigger("reset");
//     $("#addSprintForm").find(".text-danger").text("");
// });
// });
let viewModel;
$(document).ready(function () {
    viewModel = new TaskViewModel();
    ko.applyBindings(viewModel);
    viewModel.loadProjects();

    initSortable();
    // Keep your SignalR and modal logic as-is

    if (selectedProjectId) {
        viewModel.selectedProjectId(selectedProjectId);
    }
    if (selectedSprintId) {
        viewModel.selectedSprintId(selectedSprintId);
    }

    $("#addTaskModal").on('hidden.bs.modal', function () {
        $("#addTaskForm").trigger("reset");
        $("#addTaskForm").find(".text-danger").text("");
    });

    $("#addSprintModal").on('hidden.bs.modal', function () {
        $("#addSprintForm").trigger("reset");
        $("#addSprintForm").find(".text-danger").text("");
    });
});


function loadBacklogTasks(projectId) {
    ajaxCall('/Tasks/GetBacklogTasks', 'GET', JSON.stringify({ projectId: projectId }), function (html) {
        $("#backlogContainer").empty();
        $("#backlogContainer").append(html);
    });
}

// function changeSprint(sprintId) {
//     $("#addSprintButton").prop("disabled", false);
//     selectedSprintId = sprintId;
//     $("#sprintDropDown").val(sprintId);
//     $("#sprintControls").empty();
//     if (selectedSprintId) {
//         $("#assignedToMeCheck").prop("disabled", false);
//         $("#teamMembersDropDown").prop("disabled", false);
//         GetTasksBySprintId(selectedSprintId);
//         GetSprintById(selectedSprintId);
//     }
//     else {
//         $(".card-body").empty();
//         $("#assignedToMeCheck").prop("disabled", true);
//         $("#teamMembersDropDown").prop("disabled", true);
//     }
// }

function GetSprintById(sprintId) {
    ajaxCall('/Tasks/GetSprintById', 'GET', JSON.stringify({ sprintId: sprintId }), function (response) {
        if (response) {
            $("#sprintControls").empty();
            $("#sprintDuration").empty();
            if (response.status == "In Progress") {
                $("#sprintControls").append(`
                    <div class="d-flex justify-content-between">
                        <button class="btn btn-secondary" onclick="completeSprint(${sprintId})">Complete Sprint</button>
                    </div>
                `);
            }
            else if (response.status == "Not Started") {
                $("#sprintControls").append(`
                    <div class="d-flex justify-content-between">
                        <button class="btn btn-block btn-secondary text-white h-100" onclick="startSprint(${sprintId})">Start Sprint</button>
                    </div>
                `);
            }
            else if (response.status == "Completed") {
                $("#sprintControls").append(`
                    <div class="d-flex justify-content-between">
                        <div class="badge badge-success">Completed</div>
                    </div>
                `);
            }
            $("#sprintDuration").append(`
                <div class="d-flex justify-content-between">
                    <span >Duration: ${response.duration} days</span>
                </div>
            `);
        }
    });
}

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

connection.on("ReceiveMessage", (user, message) => {
    const li = document.createElement("li");
    li.textContent = `${user}: ${message}`;
    document.getElementById("messagesList").appendChild(li);
});

connection.on("TaskStatusChanged", (taskId, newStatus) => {
    GetTasksBySprintId(selectedProjectId);
});

connection.on("NewTaskAdded", () => {
    GetTasksBySprintId(selectedProjectId);
});

connection.on("TaskUpdated", () => {
    GetTasksBySprintId(selectedProjectId);
});

connection.on("TaskDeleted", () => {
    GetTasksBySprintId(selectedProjectId);
});

connection.start().catch(err => console.error(err.toString()));

function sendMessage() {
    const user = document.getElementById("userInput").value;
    const message = document.getElementById("messageInput").value;
    connection.invoke("SendMessage", user, message).catch(err => console.error(err.toString()));
}

// function GetTasksBySprintId(selectedSprintId, userId) {
//     const data = JSON.stringify({ sprintId: selectedSprintId, userId: userId || 0 });

//     ajaxCall('/Tasks/GetTasksBySprintId', 'GET', data, function (html) {
//         $("#taskListContainer").empty().append(html);
//         initSortable();
//     });
// }

function initSortable() {
    $(".task-column").sortable({
        connectWith: ".task-column",
        items: ".task-card",
        placeholder: "task-placeholder",
        receive: function (event, ui) {
            const taskId = ui.item.data("task-id");
            const oldStatus = ui.sender.data("status");
            const newStatus = $(this).data("status");

            // Define allowed transitions
            const allowedTransitions = {
                "To Do": ["In Progress"],
                "In Progress": ["To Do", "Testing"],
                "Testing": ["In Progress", "Done"]
            };

            if (!allowedTransitions[oldStatus] || !allowedTransitions[oldStatus].includes(newStatus)) {
                $(ui.sender).sortable('cancel');
                toastr.warning(`Tasks from "${oldStatus}" cannot be moved to "${newStatus}".`);
                return;
            }

            ajaxCall('/Tasks/UpdateStatus', 'POST', JSON.stringify({ taskId, newStatus }), function (response) {
                if (response.success) {
                    toastr.success(response.message);
                } else {
                    toastr.error(response.message);
                }
            });
        }
    }).disableSelection();
}

function GetTasksByProjectId(selectedProjectId, userId) {
    const data = JSON.stringify({ projectId: selectedProjectId, userId: userId || 0 });

    ajaxCall('/Tasks/GetTasksByProjectId', 'GET', data, function (html) {
        $("#taskListContainer").empty().append(html);
        initSortable();
    });
}

function openAddSprintModal() {
    const projectId = viewModel.selectedProjectId();

    if (!projectId) {
        toastr.error("Please select a project");
        return;
    }

    ajaxCall('/Tasks/AddSprint', 'GET', JSON.stringify({ projectId: projectId }), function (response) {
        $("#addSprintModal").html(response);
        $("#addSprintModal").modal('show');
        $.validator.unobtrusive.parse("#addSprintForm");
    });
}

function openAddTaskModal() {
    const projectId = viewModel.selectedProjectId();

    if (!projectId) {
        toastr.error("Please select a project");
        return;
    }
    ajaxCall('/Tasks/AddTask', 'GET', JSON.stringify({ projectId: projectId }), function (response) {
        if (tinymce.get('addTaskDescription')) {
            tinymce.get('addTaskDescription').remove();
        }
        $("#addTaskModal").html(response);
        $("#addTaskModal").modal('show');
        $.validator.unobtrusive.parse("#addTaskForm");

        const startDateInput = $('#addTaskStartDate');

        startDateValidation = startDateInput.attr('min');
        endDateValidation = startDateInput.attr('max');
    });
}

function openAddTaskInputFile() {
    const fileUpload = $("#addTaskInputFile");
    fileUpload.click();
}

function openEditTaskInputFile() {
    const fileUpload = $("#editTaskInputFile");
    fileUpload.click();
}

function openEditTaskModal(taskId) {
    ajaxCall('/Tasks/GetTaskById', 'GET', JSON.stringify({ taskId: taskId }), function (response) {
        if (tinymce.get('editTaskDescription')) {
            tinymce.get('editTaskDescription').remove();
        }
        $("#editTaskModal").html(response);
        $("#editTaskModal").modal('show');
        const startDateInputEdit = $('#editTaskStartDate');

        startDateValidationEdit = startDateInputEdit.attr('min');
        endDateValidationEdit = startDateInputEdit.attr('max');

        if (!window.TaskPermissions.canAddEdit) {
            $("#editTaskUpload").prop("disabled", true);
            $("#editTaskForm")
                .find("input, select, textarea, button[type='submit']")
                .attr("disabled", true);
        }

        $.validator.unobtrusive.parse("#editTaskForm");

        tinymce.init({
            selector: '#editTaskDescription',
            height: 200,
            menubar: false,
            plugins: 'lists link image',
            toolbar: 'undo redo | formatselect | bold italic underline | alignleft aligncenter alignright | bullist numlist ',
            branding: false,
            statusbar: false,
            setup: function (editor) {
                const maxLength = 1000;

                editor.on('keydown', function (e) {
                    const content = editor.getContent({ format: 'text' });

                    const navigationalKeys = [
                        'Backspace', 'Delete', 'ArrowLeft', 'ArrowRight',
                        'ArrowUp', 'ArrowDown', 'Control', 'Meta', 'Alt'
                    ];

                    if (content.length >= maxLength && !navigationalKeys.includes(e.key)) {
                        e.preventDefault();
                    }
                });

                editor.on('blur', function (e) {
                    const content = editor.getContent({ format: 'text' });

                    const navigationalKeys = [
                        'Backspace', 'Delete', 'ArrowLeft', 'ArrowRight',
                        'ArrowUp', 'ArrowDown', 'Control', 'Meta', 'Alt'
                    ];

                    if (content.length >= maxLength && !navigationalKeys.includes(e.key)) {
                        editor.setContent(content.substring(0, 1000), { format: 'raw' });
                        e.preventDefault();
                        return false;
                    }
                });
            }
        });
    });
}

function openDeleteTaskModal(taskId) {
    $("#deleteTaskModal").modal('show');
    $("#deleteTaskLink").attr("href", `/Tasks/DeleteTask?taskId=${taskId}`);
}

const maxTotalSize = 25 * 1024 * 1024;
const allowedTypes = [
    'image/jpeg', 'image/png', 'image/gif', 'image/webp',
    'application/pdf',
    'application/msword',
    'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
    'application/vnd.ms-excel',
    'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    'text/plain'
];

$(document).on('change', '#addTaskInputFile', function () {
    const files = this.files;
    let totalSize = 0;
    const fileNamesList = $('#addTaskFileNamesList');

    for (let i = 0; i < files.length; i++) {
        totalSize += files[i].size;

        if (!allowedTypes.includes(files[i].type)) {
            toastr.error(` ${files[i].name} file is not allowed.`);
            this.value = "";
            return;
        }
        fileNamesList.append(`<div class="me-2"><i class="fa-solid fa-file"></i>${files[i].name}</div>`);
    }

    if (totalSize > maxTotalSize) {
        toastr.error('Total file size exceeds 25 MB.');
        this.value = "";
    }
});

$(document).on('change', '#editTaskInputFile', function () {
    const files = this.files;
    let totalSize = 0;
    const fileNamesList = $('#editTaskFileNamesList');

    for (let i = 0; i < files.length; i++) {
        totalSize += files[i].size;

        if (!allowedTypes.includes(files[i].type)) {
            toastr.error(`${files[i].name} file is not allowed.`);
            this.value = "";
            return;
        }
        fileNamesList.append(`<div class="me-2"><i class="fa-solid fa-file"></i>${files[i].name}</div>`);
    }

    if (totalSize > maxTotalSize) {
        toastr.error('Total file size exceeds 25 MB.');
        this.value = "";
    }
});

$(document).on('change', '#addTaskStartDate', function () {
    const fromDate = $(this).val();
    if (fromDate) {
        $("#addTaskDueDate").attr("min", fromDate);
    } else {
        $("#addTaskDueDate").attr("min", startDateValidation);
    }
});

$(document).on('change', '#addTaskDueDate', function () {
    const toDate = $(this).val();
    if (toDate) {
        $("#addTaskStartDate").attr("max", toDate);
    } else {
        $("#addTaskStartDate").attr("max", endDateValidation);
    }
});

$(document).on('change', '#editTaskStartDate', function () {
    var fromDate = $(this).val();
    if (fromDate) {
        $("#editTaskDueDate").attr("min", fromDate);
    } else {
        $("#editTaskDueDate").attr("min", startDateValidationEdit);
    }
});

$(document).on('change', '#editTaskDueDate', function () {
    var toDate = $(this).val();
    if (toDate) {
        $("#editTaskStartDate").attr("max", toDate);
    } else {
        $("#editTaskStartDate").attr("max", endDateValidationEdit);
    }
});

// $("#assignedToMeCheck").on('change', function () {
//     const isChecked = $(this).is(':checked');
//     if (isChecked) {
//         GetTasksBySprintId(selectedSprintId, $(this).data("user-id"));
//         $("#teamMembersDropDown").prop('disabled', true);
//         $("#teamMembersDropDown").val($(this).data("user-id"));
//     }
//     else {
//         GetTasksBySprintId(selectedSprintId);
//         $("#teamMembersDropDown").prop('disabled', false);
//         $("#teamMembersDropDown").val("");
//     }
// });

// $("#teamMembersDropDown").on('change', function () {
//     const selectedUserId = $(this).val();
//     if (selectedUserId) {
//         if (selectedUserId == userId) {
//             $("#assignedToMeCheck").prop("checked", true);
//         }
//         else {
//             $("#assignedToMeCheck").prop("checked", false);
//         }
//         GetTasksBySprintId(selectedSprintId, selectedUserId);
//     } else {
//         GetTasksBySprintId(selectedSprintId);
//         $("#assignedToMeCheck").prop("checked", false);
//     }
// });

// function loadTeamMembers(projectId) {
//     ajaxCall('/Tasks/GetTeamMembersJson', 'GET', JSON.stringify({ projectId: projectId }), function (response) {
//         const dropdown = $("#teamMembersDropDown");
//         dropdown.empty();
//         dropdown.append('<option value="">Select a Team Member</option>');
//         response.forEach(member => {
//             dropdown.append(`<option value="${member.userId}">${member.name}</option>`);
//         });
//     });
// }

// function loadSprints(projectId) {
//     ajaxCall('/Tasks/GetSprintsJson', 'GET', JSON.stringify({ projectId: projectId }), function (response) {
//         const dropdown = $("#sprintDropDown");
//         dropdown.empty();
//         dropdown.append('<option value="">Select a Sprint</option>');
//         $("#sprintControls").empty();

//         response.forEach(sprint => {
//             const startDate = new Date(sprint.startDate);
//             const endDate = new Date(sprint.endDate);
//             const duration = Math.ceil((endDate - startDate) / (1000 * 60 * 60 * 24));

//             if (sprint.status === "In Progress") {
//                 dropdown.append(`<option value="${sprint.sprintId}" selected>${sprint.name}</option>`);
//                 changeSprint(sprint.sprintId);
//             } else {
//                 dropdown.append(`<option value="${sprint.sprintId}">${sprint.name}</option>`);
//             }
//         });
//     });
// }

function startSprint(sprintId) {
    ajaxCall('/Tasks/StartSprint', 'POST', JSON.stringify({ sprintId: sprintId }), function (response) {
        if (response.success) {
            toastr.success(response.message);
            changeSprint(sprintId);
        } else {
            toastr.error(response.message);
        }
    });
}

function completeSprint(sprintId) {
    ajaxCall('/Tasks/CompleteSprint', 'POST', JSON.stringify({ sprintId: sprintId }), function (response) {
        if (response.success) {
            toastr.success(response.message);
            changeSprint(sprintId);
        } else {
            toastr.error(response.message);
        }
    });
}

function TaskViewModel() {
    const self = this;

    self.projects = ko.observableArray([]);
    self.sprints = ko.observableArray([]);
    self.teamMembers = ko.observableArray([]);
    self.tasks = ko.observableArray([]);

    self.selectedProjectId = ko.observable();
    self.selectedSprintId = ko.observable();
    self.selectedUserId = ko.observable();
    self.assignedToMe = ko.observable(false);

    self.loadProjects = function () {
        ajaxCall('/Tasks/GetProjectsJson', 'GET', null, function (data) {
            self.projects(data);
        });
    };

    self.loadSprints = function () {
        if (!self.selectedProjectId()) return;
        ajaxCall('/Tasks/GetSprintsJson', 'GET', JSON.stringify({ projectId: self.selectedProjectId() }), function (data) {
            self.sprints(data);
            const inProgress = data.find(x => x.status === "In Progress");
            if (inProgress) self.selectedSprintId(inProgress.sprintId);
        });
    };

    self.loadTeamMembers = function () {
        if (!self.selectedProjectId()) return;
        ajaxCall('/Tasks/GetTeamMembersJson', 'GET', JSON.stringify({ projectId: self.selectedProjectId() }), function (data) {
            self.teamMembers(data);
        });
    };

    self.loadTasks = function () {
        if (!self.selectedSprintId()) return;
        const uid = self.assignedToMe() ? userId : (self.selectedUserId() || 0);
        ajaxCall('/Tasks/GetTasksBySprintId', 'GET', JSON.stringify({ sprintId: self.selectedSprintId(), userId: uid }), function (html) {
            $("#taskListContainer").html(html);
            initSortable();
        });
    };

    // Observables subscriptions
    self.selectedProjectId.subscribe(function (newVal) {
        debugger;
        if (newVal) {
            self.loadSprints();
            self.loadTeamMembers();
            loadBacklogTasks(newVal);
        } else {
            self.sprints([]);
            self.teamMembers([]);
            self.tasks([]);
            // $("#taskListContainer").empty();
            // $("#backlogContainer").empty();
        }
    });

    self.selectedSprintId.subscribe(function (newSprintId) {
        self.loadTasks();

        if (newSprintId) {
            GetSprintById(newSprintId);
        } else {
            $("#sprintControls").empty();
            $("#sprintDuration").empty();
        }
    });

    self.selectedUserId.subscribe(function () {
        self.loadTasks();
    });

    self.assignedToMe.subscribe(function (checked) {
        if (checked) {
            self.selectedUserId(userId);
        } else {
            self.selectedUserId(null);
        }
    });
}
