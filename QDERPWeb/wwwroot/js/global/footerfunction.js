

function exportGridToPDF() {
    const { jsPDF } = window.jspdf;
    const doc = new jsPDF('l', 'mm', 'a4'); // 'l' for landscape, 'mm' for millimeters, 'a4' for A4 size

    // Get the instance of the DataGrid
    var dataGrid = $("#dataGrid").dxDataGrid("instance");

    // Extract headers
    var headers = dataGrid.getVisibleColumns().map(column => column.caption);

    // Extract data and calculate sums
    var creditSum = 0;
    var debitSum = 0;
    var data = dataGrid.getVisibleRows().map(row => {
        return dataGrid.getVisibleColumns().map(column => {
            let value = row.data[column.dataField];
            if (column.dataType === 'date' && value) {
                // Format date as dd-MMM-yyyy
                let date = new Date(value);
                let day = String(date.getDate()).padStart(2, '0');
                let month = date.toLocaleString('default', { month: 'short' });
                let year = date.getFullYear();
                return `${day}-${month}-${year}`;
            }
            if (column.dataField === 'CrAmount') {
                creditSum += value || 0;
            }
            if (column.dataField === 'DrAmount') {
                debitSum += value || 0;
            }
            return value;
        });
    });

    // Generate PDF
    doc.autoTable({
        head: [headers],
        body: data,
        styles: { overflow: 'linebreak' },
        theme: 'grid',
        tableWidth: 'auto', // Adjust table width to fit the page
        columnStyles: {
            0: { cellWidth: 'wrap' }, // Adjust the first column width
            // Add more column styles if needed
        },
        margin: { top: 10, right: 10, bottom: 30, left: 10 }, // Adjust margins if needed
        didDrawPage: function (data) {
            // Remove the "DataGrid Export" text
            // doc.setFontSize(10);
            // doc.text('DataGrid Export', data.settings.margin.left, 10);

            // Add footer
            var pageCount = doc.internal.getNumberOfPages();
            doc.setFontSize(8);
            doc.text(`Page ${data.pageNumber} of ${pageCount}`, data.settings.margin.left, doc.internal.pageSize.height - 20);

            // Add sum of credit and debit, and row count
            doc.text(`Total Rows: ${dataGrid.getVisibleRows().length}`, data.settings.margin.left, doc.internal.pageSize.height - 15);
            doc.text(`Total Credit: ${creditSum.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`, data.settings.margin.left + 50, doc.internal.pageSize.height - 15);
            doc.text(`Total Debit: ${debitSum.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`, data.settings.margin.left + 150, doc.internal.pageSize.height - 15);
        }
    });

    // Open PDF in a new window
    var string = doc.output('datauristring');
    var newWindow = window.open();
    newWindow.document.write('<iframe width="100%" height="100%" src="' + string + '"></iframe>');
}



// Hide context menu on click outside
let isHeaderFilterEnabled = false;

function toggleHeaderFilter() {
    var dataGrid = $("#dataGrid").dxDataGrid("instance");

    isHeaderFilterEnabled = !isHeaderFilterEnabled;
    dataGrid.option("headerFilter.visible", isHeaderFilterEnabled); // Toggle the header filter visibility
}
$(document).on("click", function () {
    var contextMenu = $("#contextMenu");
    contextMenu.hide();
    var $submenu = $('#submenuContainer');
    $submenu.hide(); // Hide the submenu popup
});

// Handle the file selection
$("#loadLayoutInput").on("change", function (event) {
    const dataGrid = $("#dataGrid").dxDataGrid("instance");
    const file = event.target.files[0];

    if (file) {
        const reader = new FileReader();

        reader.onload = function (e) {
            try {
                // Parse the JSON string from the file
                const layout = JSON.parse(e.target.result);

                // Restore the grid state
                dataGrid.state(layout);
            } catch (error) {
                console.error("Error parsing JSON:", error);
                alert("Failed to load layout. Please ensure the file is valid JSON.");
            }
        };

        // Read the file as text
        reader.readAsText(file);
    }
});
//function saveLayoutClick(e) {

//    var grid = $("#dataGrid").dxDataGrid("instance");
//    var layout = grid.state();
//    const layoutJson = JSON.stringify(layout, null, 2); // Pretty-print JSON
//    //console.log(layoutJson);
//    // Create a Blob from the JSON string
//    const blob = new Blob([layoutJson], { type: "application/json" });
//    // Create a URL for the Blob
//    const url = URL.createObjectURL(blob);
//    // Create a link element
//    const a = document.createElement("a");
//    a.href = url;
//    a.download = "grid-layout.json"; // Set the file name for download
//    // Append the link to the body (needed for Firefox)
//    document.body.appendChild(a);
//    // Programmatically click the link to trigger the download
//    a.click();
//    // Clean up by removing the link and revoking the Object URL
//    document.body.removeChild(a);
//    URL.revokeObjectURL(url);
//    //DevExpress.ui.notify("Layout", "success", 600);
//}
//function openLayoutClick(e) {

//    // Create an input element dynamically
//    const input = document.createElement("input");
//    input.type = "file";
//    input.accept = "application/json";

//    // When a file is selected
//    input.addEventListener("change", function (event) {
//        const file = event.target.files[0];
//        if (!file) return;

//        const reader = new FileReader();
//        reader.onload = function (e) {
//            try {
//                const layout = JSON.parse(e.target.result);
//                const grid = $("#dataGrid").dxDataGrid("instance");
//                grid.state(layout);
//                //DevExpress.ui.notify("Layout applied successfully!", "success", 600);
//            } catch (error) {
//                DevExpress.ui.notify("Invalid layout file!", "error", 600);
//            }
//        };

//        reader.readAsText(file);
//    });

//    // Trigger the file input click event
//    input.click();
//}

//function setDefaultLayoutClick(e) {
//    const grid = $("#dataGrid").dxDataGrid("instance");
//    const form = $('#formId').val();

//    if (!grid) {
//        DevExpress.ui.notify("Grid not found", "error", 2000);
//        return;
//    }

//    const layout = grid.state();
//    //   const layoutJson = JSON.stringify(layout, null, 2); // JSON string
//    const layoutJson = convertLayoutToXml(layout); // Optional: to get XML string

//    // console.log("JSON Layout:", layoutJson);
//    console.log("XML Layout:", layoutJson); // Comment if not needed

//    $.ajax({
//        url: '/api/Utility/SaveLayout',
//        type: 'POST',
//        contentType: 'application/json',
//        data: JSON.stringify({
//            layout: layoutJson, // or use layoutXml if backend expects XML
//            form: form
//        }),
//        success: function (data) {
//            DevExpress.ui.notify("Layout saved successfully", "success", 2000);
//        },
//        error: function (xhr) {
//            console.error("SaveLayout error:", xhr.responseText);
//            DevExpress.ui.notify("Error while saving data", "error", 2000);
//        }
//    });
//}

// Optional: Convert DevExtreme JSON to custom XML format (not real XtraSerializer)
function convertLayoutToXml(layout) {
    let xml = `<XtraSerializer version="1.0" application="View">\n`;
    xml += `  <property name="columns">\n`;

    layout.columns?.forEach(col => {
        xml += `    <property name="${col.dataField}" iskey="true">\n`;
        xml += `      <property name="DataType">${col.dataType || ''}</property>\n`;
        xml += `      <property name="VisibleIndex">${col.visibleIndex ?? -1}</property>\n`;
        xml += `      <property name="Visible">${col.visible}</property>\n`;
        xml += `    </property>\n`;
    });

    xml += `  </property>\n</XtraSerializer>`;
    return xml;
}




//function resetLayoutClick(e) {
//    var grid = $("#dataGrid").dxDataGrid("instance");
//    var form = $('#formId').val();

//    const layout = grid.state();
//    //   const layoutJson = JSON.stringify(layout, null, 2); // JSON string
//    const layoutJson = convertLayoutToXml(layout); // Optional: to get XML string

//    // console.log("JSON Layout:", layoutJson);
//    console.log("XML Layout:", layoutJson); // Comment if not needed

//    $.ajax({
//        url: '/api/Utility/SaveLayout',
//        type: 'POST',
//        contentType: 'application/json',
//        data: JSON.stringify({
//            layout: layoutJson, // or use layoutXml if backend expects XML
//            form: form
//        }),
//        success: function (data) {
//            DevExpress.ui.notify("Layout reset successfull", "success", 2000);
//            grid.state({});
//        },
//        error: function () {
//            DevExpress.ui.notify("Error while saving data", "error", 2000);
//        }
//    });
//}

// Footer Group Pannel button click event
function toggleGroupPanel() {
    var dataGrid = $("#dataGrid").dxDataGrid("instance");
    var isVisible = dataGrid.option("groupPanel.visible");
    dataGrid.option("groupPanel.visible", !isVisible);
}
function showColumnChooser() {
    const dataGrid = $("#dataGrid").dxDataGrid("instance");
    if (dataGrid) {
        dataGrid.showColumnChooser();
        sortColumnChooserAlphabetically(); // Sort after opening
    }
}




// Footer Filter row button click event
function toggleFilterRow() {
    var grid = $("#dataGrid").dxDataGrid("instance");
    var currentVisibility = grid.option("filterRow.visible");

    grid.option("filterRow.visible", !currentVisibility);
}

function ChangeRowHeight(newHeight) {
    console.log("New height:", newHeight);
    // Update the row height
    $("#dataGrid .dx-data-row").css("height", newHeight + "px");
    closeSubmenu();
}
// Handle the file selection
$("#loadLayoutInput").on("change", function (event) {
    const dataGrid = $("#dataGrid").dxDataGrid("instance");
    const file = event.target.files[0];

    if (file) {
        const reader = new FileReader();

        reader.onload = function (e) {
            try {

                const layout = JSON.parse(e.target.result);


                dataGrid.state(layout);
            } catch (error) {
                console.error("Error parsing JSON:", error);
                alert("Failed to load layout. Please ensure the file is valid JSON.");
            }
        };


        reader.readAsText(file);
    }
});

$(function () {
    const svgIcon = `
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 399.58 279.97" width="20" height="20">
                <defs>
                    <style>
                            .cls-1 { fill: var(--title-background-color); }
                        .cls-2 { fill: #a4abb8; }
                    </style>
                </defs>
                <g>
                    <path class="cls-1" d="M80.09,69.88v140h32.5c5.3,0,10.16,10.7,5.95,14.96l-52.43,52.56c-2.45,2.54-5.8,3.15-9.07,2.03L1.64,224.83c-4.4-3.99.95-14.96,5.95-14.96h32.5V69.88H7.59c-4.9,0-10.4-10.76-5.95-14.96L54.05,2.34c3.68-3.11,8.37-3.13,12.08,0l52.41,52.58c4.45,4.2-1.05,14.96-5.95,14.96h-32.5Z"/>
                    <path class="cls-2" d="M396.81,57.6c-1.35,1.36-5.16,2.32-7.17,2.33l-221.99-.12c-12.32.52-9.65-32.56-6.04-36.91,1.34-1.61,4.84-2.8,6.95-3.05l222.97.08c3.57.56,6.93,3.91,7.5,7.5.75,4.72.63,19.02.08,23.97-.18,1.66-1.2,5.09-2.31,6.2Z"/>
                    <path class="cls-2" d="M396.81,257.6c-1.35,1.36-5.16,2.32-7.17,2.33l-223-.11c-9.5-1.57-8.29-22.11-7.44-29.33.73-6.22,2.7-9.84,9.37-10.63l222.97.08c2.79.42,7,3.65,7.53,6.47.78,4.09.56,20.45.05,25-.18,1.66-1.2,5.09-2.31,6.2Z"/>
                    <path class="cls-2" d="M162.4,122.19c1.56-1.44,4.97-2.34,7.14-2.36l221.99.11c3.14.47,6.66,3.59,7.35,6.65.74,3.29.83,23.39.19,26.77-.76,4-6.65,6.8-10.44,6.56l-220.97-.1c-6.07-1.16-7.78-4.8-8.45-10.55s-1.2-23,3.2-27.08Z"/>
                </g>
            </svg>`;

    // Inject the SVG into the DropDownButton
    $('#rowHeightProperties .dx-button-content').prepend(svgIcon);
});
function refreshGrid() {
    const grid = $("#dataGrid").dxDataGrid("instance");
    grid.refresh();
}
//function loadGridState() {
//    var form = $('#formId').val();
//    // Call the API to get the grid state
//    $.ajax({
//        url: `/api/Utility/LoadLayout`, // Adjust to your actual API endpoint
//        method: 'GET',
//        data: {
//            form: form
//        },
//        success: function (response) {
//            // Get the DataGrid instance
//            var dataGrid = $("#dataGrid").dxDataGrid("instance");

//            // Load the state into the DataGrid
//            if (response) {
//                try {
//                    // Parse the JSON string from the file
//                    const layout = JSON.parse(response);

//                    // Restore the grid state
//                    dataGrid.state(layout);
//                } catch (error) {
//                    console.error("Error parsing JSON:", error);
//                    alert("Failed to load layout. Please ensure the JSON is valid.");
//                }
//            }
//        },
//        error: function (xhr, status, error) {
//            console.error("Error loading grid state:", error);
//        }
//    });
//}
function setDefaultLayoutClick(e) {
    const grid = $("#dataGrid").dxDataGrid("instance");
    const form = $('#formId').val();

    if (!grid) {
        DevExpress.ui.notify("Grid not found", "error", 2000);
        return;
    }

    const layout = grid.state();
    const layoutJson = JSON.stringify(layout); // ✅ Save as JSON string

    $.ajax({
        url: '/api/Utility/SaveLayout',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            layout: layoutJson,
            form: form
        }),
        success: function (data) {
            DevExpress.ui.notify("Layout saved successfully", "success", 2000);
        },
        error: function (xhr) {
            console.error("SaveLayout error:", xhr.responseText);
            DevExpress.ui.notify("Error while saving data", "error", 2000);
        }
    });
}


function resetLayoutClick(e) {
    const grid = $("#dataGrid").dxDataGrid("instance");
    const form = $('#formId').val();

    $.ajax({
        url: '/api/Utility/SaveLayout',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            layout: null, // 👈 this will trigger the backend to delete or reset
            form: form
        }),
        success: function (data) {
            DevExpress.ui.notify("Layout reset successful", "success", 2000);
            grid.state(null); // 👈 clear client-side layout too
        },
        error: function (xhr) {
            console.error(xhr.responseText);
            DevExpress.ui.notify("Error while resetting layout", "error", 2000);
        }
    });
}

function saveLayoutClick(e) {
    var grid = $("#dataGrid").dxDataGrid("instance");
    var layout = grid.state();
    const layoutJson = JSON.stringify(layout, null, 2); // Pretty-print JSON
    //console.log(layoutJson);
    // Create a Blob from the JSON string
    const blob = new Blob([layoutJson], { type: "application/json" });
    // Create a URL for the Blob
    const url = URL.createObjectURL(blob);
    // Create a link element
    const a = document.createElement("a");
    a.href = url;
    a.download = "grid-layout.json"; // Set the file name for download
    // Append the link to the body (needed for Firefox)
    document.body.appendChild(a);
    // Programmatically click the link to trigger the download
    a.click();
    // Clean up by removing the link and revoking the Object URL
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
    //DevExpress.ui.notify("Layout", "success", 600);
}
function openLayoutClick(e) {
    // Create an input element dynamically
    const input = document.createElement("input");
    input.type = "file";
    input.accept = "application/json";

    // When a file is selected
    input.addEventListener("change", function (event) {
        const file = event.target.files[0];
        if (!file) return;

        const reader = new FileReader();
        reader.onload = function (e) {
            try {
                const layout = JSON.parse(e.target.result);
                const grid = $("#dataGrid").dxDataGrid("instance");
                grid.state(layout);
                //DevExpress.ui.notify("Layout applied successfully!", "success", 600);
            } catch (error) {
                DevExpress.ui.notify("Invalid layout file!", "error", 600);
            }
        };

        reader.readAsText(file);
    });

    // Trigger the file input click event
    input.click();
}

$(document).ready(function () {
    loadGridState(); // 👈 This must be called explicitly
});
function loadGridState() {

    var form = $('#formId').val();
    // Call the API to get the grid state
    $.ajax({
        url: `/api/Utility/LoadLayout`, // Adjust to your actual API endpoint
        method: 'GET',
        data: {
            form: form
        },
        success: function (response) {
            // Get the DataGrid instance
            var dataGrid = $("#dataGrid").dxDataGrid("instance");

            // Load the state into the DataGrid
            if (response) {
                try {
                    // Parse the JSON string from the file
                    const layout = JSON.parse(response);

                    // Restore the grid state
                    dataGrid.state(layout);
                } catch (error) {
                    console.error("Error parsing JSON:", error);

                }
            }
        },
        error: function (xhr, status, error) {
            console.error("Error loading grid state:", error);
        }
    });
}
function printFun() {
    exportDataGridToPDF('dataGrid');
}