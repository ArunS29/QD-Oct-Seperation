(function ($) {
    let grid;
    $.fn.extend({
        
        customizeGridContextMenu: function () {
            this.each(function () {
                 grid = $(this).dxDataGrid("instance");

                grid.option("onContextMenuPreparing", function (e) {
                    if (e.target === "header") {
                        const customOption = createCustomOption(e.column);
                        if (customOption) {
                            e.items.push(customOption);
                        }
                    }
                });
                // Apply stored rules to the grid after initialization
                grid.on("initialized", function () {
                    applyStoredRules(grid);  // Apply the rules when the grid is initialized
                });
                // Detect when the dataSource is refreshed or changed
                //grid.on("optionChanged", function (e) {
                //    if (e.name === "dataSource") {
                //        // Reapply the stored rules whenever the dataSource changes
                //        applyStoredRules(grid);
                //    }
                //});
            });
            return this;
        },
    });

    // Helper function to apply stored rules from localStorage
    function applyStoredRules(grid) {
        let storedRules = JSON.parse(localStorage.getItem("gridRules"));

        // If there are no stored rules, remove all formatting
        if (!storedRules || storedRules.length === 0) {
            grid.option("onRowPrepared", null);  // Remove row preparation logic (highlighting)
            grid.columnOption("column", "customizeText", null);  // Remove column customization
            return; // Exit the function as no formatting should be applied
        }

        // Apply rules if they exist
        storedRules.forEach(rule => {
            // Apply custom text formatting based on the rule
            grid.columnOption(rule.column, "customizeText", function (e) {
                if (rule.condition === "Greater Than" && e.value > parseFloat(rule.value)) {
                    return `<span style="background-color: yellow;">${e.value}</span>`;
                }
                if (rule.condition === "Less Than" && e.value < parseFloat(rule.value)) {
                    return `<span style="background-color: lightgreen;">${e.value}</span>`;
                }
                // Add more conditions as necessary
                return e.value;
            });

            // Highlight the row if specified
            if (rule.highlightRow) {
                grid.option("onRowPrepared", function (e) {
                    if (e.rowType === "data" && e.data[rule.column] > parseFloat(rule.value)) {
                        e.rowElement.css("background-color", "lightyellow");
                    }
                });
            }
        });
    }

    // Helper function to create the custom option for the context menu
    function createCustomOption(column) {
        const customOption = {
            text: "Conditional Formatting",
            items: []
        };

        const dataTypeOptions = {
            number: [
                { text: "Highlight Cell Rules", items: createNumberRules(column) },
                { text: "Unique/Duplicate Rules", items: createUniqueDuplicateRules("number", column) },
                { text: "Top/Bottom Rules", items: createTopBottomRules("number", column) }
            ],
            string: [
                { text: "Highlight Cell Rules", items: createStringRules(column) },
                { text: "Unique/Duplicate Rules", items: createUniqueDuplicateRules("string", column) }
            ],
            date: [
                { text: "Highlight Cell Rules", items: createDateRules(column) },
                { text: "Unique/Duplicate Rules", items: createUniqueDuplicateRules("date", column) }
            ]
        };

        if (dataTypeOptions[column.dataType]) {
            customOption.items = dataTypeOptions[column.dataType];
        }

        // Always add the Manage Rules option
        customOption.items.push({
            text: "Manage Rules",
            onItemClick: openManageRulesPopup
        });

        return customOption.items.length > 0 ? customOption : null;
    }

    // Helper functions to generate rule items based on data type
    function createNumberRules(column) {
        return [
            "Greater Than", "Greater Than or Equal To", "Less Than", "Less Than or Equal To", "Equal To"
        ].map(rule => createRuleItem(column,rule, "number"));
    }

    function createStringRules(column) {
        return [
            "Contains", "Starts With"
        ].map(rule => createRuleItem(column,rule, "string"));
    }

    function createDateRules(column) {
        return [
            "Greater Than", "Less Than", "Between"
        ].map(rule => createRuleItem(column, rule, "date"));
    }

    function createTopBottomRules(column, type) {
        return [
            "Top 10 Items", "Top 10% Items", "Bottom 10 Items", "Bottom 10% Items", "Above Average", "Below Average"
        ].map(rule => createRuleItem(column,rule, type));
    }

    function createUniqueDuplicateRules(column,type) {
        return [
            "Unique Values", "Duplicate Values"
        ].map(rule => createRuleItem(column,rule, type));
    }

    // Helper function to create a rule item
    function createRuleItem(column,condition, type) {
        return {
            text: condition,
            onItemClick: function () {
                openRuleDialog(column, condition, type);
            }
        };
    }

    // Function to open the rule dialog
    function openRuleDialog(column, condition, type) {
        let dialogHtml = `
            <div>
                <p><strong>Condition:</strong> ${condition}</p>
                <div>
                    <input type="text" id="ruleValue" placeholder="Enter Value">
                    <select id="dropdown" style="margin-left: 10px;">
                        <option value="option1">Option 1</option>
                        <option value="option2">Option 2</option>
                        <option value="option3">Option 3</option>
                    </select><br><br>
                </div>
                <label><input type="checkbox" id="highlightRow"> Highlight entire row</label><br><br>
            </div>
            <div class="popup-buttons" style='text-align:right;'>
                <button id="saveButton" class="dx-button dx-widget dx-button-mode-contained">Save</button>
                <button id="cancelButton" class="dx-button dx-widget dx-button-mode-contained">Cancel</button>
            </div>
        `;

        let $dialogContainer = $('<div id="ruleDialogContainer"></div>').html(dialogHtml).appendTo("body");

        $dialogContainer.dxPopup({
            title: `Enter Rule for ${column.caption}`,
            visible: false,
            showTitle: true,
            width: 400,
            height: "auto",
            dragEnabled: true,
            closeOnOutsideClick: true,
            onHidden: function () { $dialogContainer.remove(); applyStoredRules(grid); }
        }).dxPopup("show");

        $("#saveButton").dxButton({
            text: "Save",
            type: "success",
            onClick: function () {
                saveRule(column, condition, type);
                $dialogContainer.dxPopup("hide");
            }
        });

        $("#cancelButton").dxButton({
            text: "Cancel",
            type: "normal",
            onClick: function () { $dialogContainer.dxPopup("hide"); }
        });
    }

    // Function to save the rule to localStorage
    function saveRule(column, condition, type) {
        let ruleValue = $("#ruleValue").val();
        let highlightRow = $("#highlightRow").prop("checked");

        let rules = JSON.parse(localStorage.getItem("gridRules")) || [];
        rules.push({ column: column.caption, condition, value: ruleValue, type, highlightRow });
        localStorage.setItem("gridRules", JSON.stringify(rules));
    }

    // Function to open the "Manage Rules" popup
    function openManageRulesPopup() {
        let storedRules = JSON.parse(localStorage.getItem("gridRules")) || [];
        let $dialogContainer = $('<div id="manageRulesDialogContainer"></div>')
            .html('<div id="rulesGrid"></div><div style="text-align: right;"><button id="deleteAllButton" class="dx-button dx-widget dx-button-mode-contained">Delete All</button></div>')
            .appendTo("body");

        $dialogContainer.dxPopup({
            title: "Manage Conditional Formatting Rules",
            visible: false,
            showTitle: true,
            width: 600,
            height: 400,
            dragEnabled: true,
            closeOnOutsideClick: true,
            onHidden: function () { $dialogContainer.remove(); applyStoredRules(grid); }
        }).dxPopup("show");

        $("#rulesGrid").dxDataGrid({
            dataSource: storedRules,
            columns: [
                { dataField: "column", caption: "Column" },
                { dataField: "condition", caption: "Condition" },
                { dataField: "value", caption: "Value" },
                { dataField: "highlightRow", caption: "Highlight Row", dataType: "boolean" },
                {
                    type: "buttons",
                    buttons: [{ hint: "Delete", icon: "trash", onClick: (e) => deleteRule(e.row.data) }]
                }
            ],
            paging: { pageSize: 10 },
            pager: { showPageSizeSelector: true, allowedPageSizes: [10, 20, 50] }
        });

        $("#deleteAllButton").dxButton({
            text: "Delete All",
            type: "danger",
            onClick: deleteAllRules
        });
    }

    // Function to update the data source of the rules grid
    function updateGridDataSource() {
        let storedRules = JSON.parse(localStorage.getItem("gridRules")) || [];
        $("#rulesGrid").dxDataGrid("instance").option("dataSource", storedRules);
    }

    // Function to delete a specific rule
    function deleteRule(rule) {
        let rules = JSON.parse(localStorage.getItem("gridRules")) || [];
        rules = rules.filter(r => r.column !== rule.column || r.value !== rule.value || r.condition !== rule.condition);
        localStorage.setItem("gridRules", JSON.stringify(rules));
        updateGridDataSource();
    }

    // Function to delete all rules
    function deleteAllRules() {
        localStorage.removeItem("gridRules");
        updateGridDataSource();
    }

})(jQuery);
