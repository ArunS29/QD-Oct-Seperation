(function ($) {
    let grid;
    $.fn.extend({
        customizeGridContextMenu: function () {
            this.each(function () {
                grid = $(this).dxDataGrid("instance");

                grid.option("onContextMenuPreparing", handleContextMenuPreparing);
                grid.on("initialized", () => applyStoredRules(grid));
                grid.on("optionChanged", handleOptionChanged);
            });
            return this;
        },
    });
    // Event Handlers
    function handleContextMenuPreparing(e) {
        if (e.target === "header") {
            const customOption = createCustomOption(e.column);
            if (customOption) e.items.push(customOption);
        }
    }

    function handleOptionChanged(e) {
        if (e.name === "dataSource") {
            applyStoredRules(grid);
        }
    }
    // Helper function to apply stored rules from localStorage
    function applyStoredRules(grid) {
        const storedRules = JSON.parse(localStorage.getItem("gridRules")) || [];

        if (!storedRules.length) {
            grid.option({ onCellPrepared: null, onRowPrepared: null });
            return;
        }

        grid.option("onCellPrepared", (e) => applyCellFormatting(e, storedRules));
        grid.option("onRowPrepared", (e) => applyRowFormatting(e, storedRules));
    }

    function applyCellFormatting(e, rules) {
        if (e.rowType !== "data") return;

        rules.forEach((rule) => {
            if (e.column.dataField === rule.column) {
                const cellValue = e.value;
                const allValues = getColumnValues(grid, rule.column);

                if (checkCondition(cellValue, allValues, rule)) {
                    applyFormattingStyles(e.cellElement, rule.formattingStyle);
                }
            }
        });
    }

    function applyRowFormatting(e, rules) {
        if (e.rowType !== "data") return;

        rules.forEach((rule) => {
            if (rule.highlightRow) {
                const cellValue = e.data[rule.column];
                const allValues = getColumnValues(grid, rule.column);

                if (checkCondition(cellValue, allValues, rule)) {
                    applyFormattingStyles(e.rowElement, rule.formattingStyle);
                }
            }
        });
    }

    function getColumnValues(grid, column) {
        return grid.getDataSource().items().map((item) => item[column]);
    }

    function checkCondition(value, allValues, rule) {
        switch (rule.condition) {
            case "Duplicate Values":
                return allValues.filter((v) => v === value).length > 1;
            case "Unique Values":
                return allValues.filter((v) => v === value).length === 1;
            case "Top 10 Items":
                return isInTopOrBottom(value, allValues, 10, "desc");
            case "Bottom 10 Items":
                return isInTopOrBottom(value, allValues, 10, "asc");
            case "Top 10% Items":
                return isInTopOrBottomPercent(value, allValues, 0.1, "desc");
            case "Bottom 10% Items":
                return isInTopOrBottomPercent(value, allValues, 0.1, "asc");
            case "Above Average":
                return value > average(allValues);
            case "Below Average":
                return value < average(allValues);
            case "Contains":
                return value.includes(rule.value);
            case "Equals":
                return value.toLowerCase() === rule.value.toLowerCase();
            case "Starts With":
                return value.toLowerCase().startsWith(rule.value.toLowerCase());
            case "IsBlank":
                return typeof value == 'string' && !value.trim() || typeof value == 'undefined' || value === null;
            case "IsNotBlank":
                return !(typeof value == 'string' && !value.trim() || typeof value == 'undefined' || value === null);
            case "After":
                return new Date(value) > new Date(rule.value);
            case "Before":
                return new Date(value) < new Date(rule.value);
            case "Between":
                if (!rule.value.includes(";")) {
                    console.error("Invalid 'Between' rule value format. Expected 'fromDate;toDate'.");
                    return false;
                }
                const [fromDate, toDate] = rule.value.split(";").map((date) => new Date(date));
                const valueDate = new Date(value);
                return valueDate >= fromDate && valueDate <= toDate;
            default:
                return compareValues(value, rule.value, rule.condition);
        }
    }

    function isInTopOrBottom(value, allValues, count, order) {
        const sortedValues = [...allValues].sort((a, b) => (order === "desc" ? b - a : a - b));
        return sortedValues.slice(0, count).includes(value);
    }

    function isInTopOrBottomPercent(value, allValues, percent, order) {
        const threshold = Math.ceil(allValues.length * percent);
        return isInTopOrBottom(value, allValues, threshold, order);
    }

    function average(values) {
        return values.reduce((sum, v) => sum + v, 0) / values.length;
    }

    function compareValues(value, ruleValue, condition) {
        const sanitizedValue = parseFloat(value.toString().replace(/,/g, ''));
        const numericValue = parseFloat(ruleValue);
        switch (condition) {
            case "Greater Than":
                return sanitizedValue > numericValue;
            case "Less Than":
                return sanitizedValue < numericValue;
            case "Equal To":
                return sanitizedValue == numericValue;
            case "Greater Than or Equal To":
                return sanitizedValue >= numericValue;
            case "Less Than or Equal To":
                return sanitizedValue <= numericValue;
            default:
                return false;
        }
    }

    function applyFormattingStyles(element, styles) {
        styles.split(";").forEach((style) => {
            const [property, value] = style.split(":").map((s) => s.trim());
            if (property && value) $(element).css(property, value);
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
                { text: "Unique/Duplicate Rules", items: createUniqueDuplicateRules(column, "number") },
                { text: "Top/Bottom Rules", items: createTopBottomRules(column, "number") }
            ],
            string: [
                { text: "Highlight Cell Rules", items: createStringRules(column) },
                { text: "Unique/Duplicate Rules", items: createUniqueDuplicateRules(column, "string") }
            ],
            date: [
                { text: "Highlight Cell Rules", items: createDateRules(column) },
                { text: "Unique/Duplicate Rules", items: createUniqueDuplicateRules(column, "date") }
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
        ].map(rule => createRuleItem(column, rule, "number"));
    }

    function createStringRules(column) {
        return [
            "Contains", "Starts With", "Equals", "IsBlank", "IsNotBlank"
        ].map(rule => createRuleItem(column, rule, "string"));
    }

    function createDateRules(column) {
        return [
            "After", "Before", "Between"
        ].map(rule => createRuleItem(column, rule, "date"));
    }

    function createTopBottomRules(column, type) {
        return [
            "Top 10 Items", "Top 10% Items", "Bottom 10 Items", "Bottom 10% Items", "Above Average", "Below Average"
        ].map(rule => createRuleItem(column, rule, type));
    }

    function createUniqueDuplicateRules(column, type) {
        return [
            "Unique Values", "Duplicate Values"
        ].map(rule => createRuleItem(column, rule, type));
    }

    // Helper function to create a rule item
    function createRuleItem(column, condition, type) {
        return {
            text: condition,
            onItemClick: function () {
                openRuleDialog(column, condition, type);
            }
        };
    }

    // Function to open the rule dialog
    function openRuleDialog(column, condition, type) {
        const isNonValueRule = (condition === "Duplicate Values"
            || condition === "Unique Values"
            || condition === "Top 10 Items"
            || condition === "Top 10% Items"
            || condition === "Bottom 10 Items"
            || condition === "Bottom 10% Items"
            || condition === "Above Average"
            || condition === "Below Average"
            || condition === "IsBlank"
            || condition === "IsNotBlank"
            || condition === "Before"
            || condition === "After"
            || condition === "Between"
        );
        const isDateRule = (condition === "Before"
            || condition === "After"
            || condition === "Between"
        );

        let dialogHtml = `
        <div>
            <p><strong>Condition:</strong> ${condition}</p>
            <div>
            <div ${isNonValueRule ? 'style="display: none;"' : 'style=" float:left;"'} id="valueInputSection">
                <input type="text" id="ruleValue" placeholder="Enter Value">
            </div>
            <div ${!isDateRule ? 'style="display: none;"' : 'style=" float:left;"'} id="datevalueInputSection">
            <div ${condition !== "Between" ? '' : 'style="display: none;"'} id="singleDateSection">
                    <input type="date" id="singleDate">
                </div>
                <div ${condition === "Between" ? 'style=" float:left;"' : 'style="display: none;"'} id="betweenDateSection">
                    <label>From: <input type="date" id="fromDate"></label><br>
                    <label>To: <input style="margin-top: 15px;margin-left: 18px;" type="date" id="toDate"></label>
                </div>
            </div>
            <div>
                <select id="ddformattingStyle" style="margin-left: 10px;">
                    <option value="font-weight: bold;">Bold Text</option>
                    <option value="color: green; font-weight: bold;">Green Bold Text</option>
                    <option value="background-color: green;">Green Fill</option>
                    <option value="background-color: green; color: white;">Green Fill with White Text</option>
                    <option value="color: green;">Green Text</option>
                    <option value="color: red; font-weight: bold;">Red Bold Text</option>
                    <option value="background-color: red;">Red Fill</option>
                    <option value="background-color: red; font-weight: bold; color: white;">Red Fill with White Bold Text</option>
                    <option value="color: red;">Red Text</option>
                    <option value="background-color: yellow;">Yellow Fill</option>
                    <option value="background-color: yellow; font-weight: bold;">Yellow Fill with Bold Text</option>
                    <option value="font-style: italic;">Italic Text</option>
                    <option value="text-decoration: line-through;">Strikeout Text</option>
                </select><br><br>
            </div>
            </div>
            <div>
            <label style="margin-left:10px;"><input type="checkbox" id="highlightRow"> Highlight entire row</label><br><br>
            </div>
        </div>
        <div class="popup-buttons" style="text-align:right;">
            <button id="saveButton" class="dx-button dx-widget dx-button-mode-contained">Save</button>
            <button id="cancelButton" class="dx-button dx-widget dx-button-mode-contained">Cancel</button>
        </div>
    `;

        let $dialogContainer = $('<div id="ruleDialogContainer"></div>').html(dialogHtml).appendTo("body");

        $dialogContainer.dxPopup({
            title: `Enter Rule for ${column.caption}`,
            visible: false,
            showTitle: true,
            width: 430,
            height: "auto",
            dragEnabled: true,
            closeOnOutsideClick: true,
            onHidden: function () { $dialogContainer.remove(); applyStoredRules(grid); }
        }).dxPopup("show");
        const popupInstance = $dialogContainer.dxPopup("instance");
        $("#saveButton").dxButton({
            text: "Save",
            type: "success",
            onClick: function () {
                const saveSuccessful = saveRule(column, condition, type, popupInstance);
                if (saveSuccessful)
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
    function saveRule(column, condition, type, popupInstance) {
        const isNonValueRule = (condition === "Duplicate Values"
            || condition === "Unique Values"
            || condition === "Top 10 Items"
            || condition === "Top 10% Items"
            || condition === "Bottom 10 Items"
            || condition === "Bottom 10% Items"
            || condition === "Above Average"
            || condition === "Below Average"
            || condition === "IsBlank"
            || condition === "IsNotBlank"
            || condition === "Before"
            || condition === "After"
            || condition === "Between"
        );
        const isDateRule = (condition === "Before"
            || condition === "After"
            || condition === "Between"
        );
        let ruleValue = condition === "Duplicate Values" || condition === "Unique Values" ? null : $("#ruleValue").val();
        if (!isNonValueRule && type === "number" && isNaN(ruleValue)) {
            alert("Please enter a valid number.");
            return false;
        }
        if (isDateRule && type === "date") {
            if (condition === "After" || condition === "Before") {
                ruleValue = $("#singleDate").val();
                if (isNaN(Date.parse(ruleValue))) {
                    alert("Please enter a valid date.");
                    return false;
                }
            }
            if (condition === "Between") {
                let fromDate = $("#fromDate").val();
                let toDate = $("#toDate").val();

                if (!fromDate || !toDate) {
                    alert("Both 'From' and 'To' dates are required.");
                    return false;
                }
                if (new Date(fromDate) > new Date(toDate)) {
                    alert("'From' date cannot be later than 'To' date.");
                    return false;
                }

                ruleValue = fromDate + ";" + toDate;
            }

        }

        //let ruleValue = $("#ruleValue").val();
        let formattingStyle = $("#ddformattingStyle").val();
        let highlightRow = $("#highlightRow").prop("checked");

        let rules = JSON.parse(localStorage.getItem("gridRules")) || [];
        rules.push({ name: column.caption, column: column.dataField, condition, value: ruleValue, type, highlightRow, formattingStyle: formattingStyle });
        localStorage.setItem("gridRules", JSON.stringify(rules));
        return true;
    }

    // Function to open the "Manage Rules" popup
    function openManageRulesPopup() {
        let storedRules = JSON.parse(localStorage.getItem("gridRules")) || [];
        let $dialogContainer = $('<div id="manageRulesDialogContainer"></div>')
            .html('<div id="rulesGrid"></div><div style="text-align: right;margin-top: 15px;"><button id="deleteAllButton" class="dx-button dx-widget dx-button-mode-contained">Delete All</button></div>')
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
                { dataField: "name", caption: "Name" },
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
