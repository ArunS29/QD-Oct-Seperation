function formatDateOnlyLocal(date) {
    if (!date) return null;
    const d = new Date(date);
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, "0");
    const day = String(d.getDate()).padStart(2, "0");
    return `${y}-${m}-${day}`; // yyyy-MM-dd
}

// Map: DateBoxID → custom blocked message
const blockedMessages = {
    "VoucherDate": "This Voucher Entry date has been blocked. Please review your entry date.",
    "VoucherEffectiveDate": "This Voucher Effective Date has been blocked. Please review your entry date.",
    "claimerdate": "This Claim Date has been blocked. Please review your entry date.",
    "InvoiceDueDate": "This Invoice Due Date has been blocked. Please review your entry date.",
    "claimerEffectiveDate": "This  Effective Date has been blocked. Please review your entry date.",
    "EffectiveDate": "This  Effective Date has been blocked. Please review your entry date.",
};

function onVoucherDateChanged(e) {
    // Only handle user-initiated changes
    if (!e || !e.event || !e.value) return;

    const id = e.element && e.element.attr ? e.element.attr("id") : null;
    const dateBox = id ? $("#" + id).dxDateBox("instance") : null;
    if (!dateBox) return;

    $.ajax({
        url: "/api/VoucherMaster/CheckVoucherDateLock",
        type: "GET",
        data: { voucherDate: formatDateOnlyLocal(e.value) },
        success: function (response) {
            if (response && response.isLocked) {
                // Pick message based on ID
                let message = blockedMessages[id]
                    || "This date has been blocked. Please review your entry date.";

                DevExpress.ui.notify(
                                     message,
                                     "error",
                                     3000
                );

                
                dateBox.option("value", new Date());
            }
        },
        error: function () {
            DevExpress.ui.notify("Error checking date lock.", "error", 3000);
        }
    });
}
