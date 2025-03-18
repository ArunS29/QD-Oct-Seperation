function exportDataGridToPDF(gridId) {
    var grid = $("#" + gridId).dxDataGrid("instance");

    if (!grid) {
        console.error("DataGrid not found!");
        return;
    }

    // Load all data (not just the visible page)
    grid.getDataSource().load().done(function (fullData) {
        const { jsPDF } = window.jspdf;

        var columns = grid.getVisibleColumns();
        var columnCount = columns.length;

        // Dynamically set orientation
        var orientation = columnCount > 10 ? "landscape" : "portrait";  // Landscape for more columns

        var doc = new jsPDF({
            orientation: orientation,
            unit: "mm",
            format: "a3"  // Always use A3 for more space
        });

        // Adjust column width dynamically based on content
        var columnStyles = {};
        columns.forEach((col, index) => {
            columnStyles[index] = { cellWidth: "wrap", minCellWidth: 20 }; // Ensures data does not shrink
        });

        DevExpress.pdfExporter.exportDataGrid({
            jsPDFDocument: doc,
            component: grid,
            autoTableOptions: {
                styles: { fontSize: 9, cellPadding: 3 },
                tableWidth: "wrap", // Ensures table wraps instead of shrinking columns
                columnStyles: columnStyles
            }
        }).then(() => {
            var pdfBlob = doc.output("blob");

            // Open a new window
            var newWindow = window.open("", "_blank", "width=1200,height=800");
            newWindow.document.write(`
                <html>
                <head>
                    <title>Exported PDF</title>
                    <script>
                        function printPDF() {
                            document.getElementById('pdfViewer').contentWindow.print();
                        }
                    <\/script>
                </head>
                <body>
                    <div style="display: flex; justify-content: space-between; padding: 10px; background: #ddd;">
                        <button onclick="printPDF()">🖨 Print</button>
                        <button onclick="window.close()">❌ Close</button>
                    </div>
                    <iframe id="pdfViewer" width="100%" height="90%" style="border:none;"></iframe>
                </body>
                </html>
            `);

            newWindow.document.close();

            var blobUrl = URL.createObjectURL(pdfBlob);
            newWindow.document.getElementById("pdfViewer").src = blobUrl;
        });
    }).fail(function () {
        console.error("Failed to load all data.");
    });
}
