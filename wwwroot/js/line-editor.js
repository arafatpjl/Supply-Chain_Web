// Line editor for documents with a header and repeating detail rows.
//
// Replaces the MSFlexGrid the VB6 forms used to build their line list. Rows are added and removed
// in the browser and posted as one collection, so the whole document arrives in a single request
// and is written in a single transaction. The legacy forms wrote the header and each line as
// separate untransacted statements.
//
// Model binding needs contiguous indexes — Lines[0], Lines[1], … — so every field is renumbered
// after any add or remove.
(function () {
    'use strict';

    const body = document.getElementById('lineBody');
    const template = document.getElementById('lineTemplate');
    const addButton = document.getElementById('addLine');
    const grandTotal = document.getElementById('grandTotal');

    if (!body || !template || !addButton) {
        return;
    }

    function renumber() {
        body.querySelectorAll('tr.po-line').forEach(function (row, index) {
            row.querySelectorAll('[name]').forEach(function (field) {
                field.name = field.name.replace(/Lines\[[^\]]*\]/, 'Lines[' + index + ']');
            });
        });
    }

    function recalculate() {
        let total = 0;

        body.querySelectorAll('tr.po-line').forEach(function (row) {
            const qty = parseFloat(row.querySelector('.po-qty')?.value) || 0;
            const price = parseFloat(row.querySelector('.po-price')?.value) || 0;
            const lineTotal = qty * price;

            const cell = row.querySelector('.po-total');
            if (cell) {
                cell.textContent = lineTotal.toFixed(2);
            }

            total += lineTotal;
        });

        if (grandTotal) {
            grandTotal.textContent = total.toFixed(2);
        }
    }

    addButton.addEventListener('click', function () {
        const markup = template.innerHTML.replace(/__i__/g, String(body.querySelectorAll('tr.po-line').length));
        const holder = document.createElement('tbody');
        holder.innerHTML = markup.trim();

        const row = holder.querySelector('tr');
        if (row) {
            body.appendChild(row);
            renumber();
            recalculate();
            row.querySelector('select, input')?.focus();
        }
    });

    body.addEventListener('click', function (event) {
        if (event.target.closest('.po-remove')) {
            event.target.closest('tr.po-line')?.remove();
            renumber();
            recalculate();
        }
    });

    // The running total updates as the user types, which the legacy grid only did on row commit.
    body.addEventListener('input', function (event) {
        if (event.target.matches('.po-qty, .po-price')) {
            recalculate();
        }
    });

    recalculate();
})();
