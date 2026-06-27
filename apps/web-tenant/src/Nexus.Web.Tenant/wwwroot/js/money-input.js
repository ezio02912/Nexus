// Live Vietnamese number formatting for text inputs.
// Thousands separator: '.'  Decimal separator: ','  (vi-VN style).
// Formats while the user types and keeps the caret in the right place.

const instances = new WeakMap();

// Count digit / decimal-comma characters (the meaningful ones) up to a position.
// Thousands dots are layout-only and are ignored so the caret stays anchored to
// the same logical character regardless of how grouping shifts the text.
const countSignificant = (text) => {
    let count = 0;
    for (const ch of text) {
        if ((ch >= "0" && ch <= "9") || ch === ",") {
            count++;
        }
    }
    return count;
};

// Find the caret index in the formatted text after `target` significant chars.
const caretFromSignificant = (formatted, target) => {
    if (target <= 0) {
        return 0;
    }
    let count = 0;
    for (let i = 0; i < formatted.length; i++) {
        const ch = formatted[i];
        if ((ch >= "0" && ch <= "9") || ch === ",") {
            count++;
            if (count === target) {
                return i + 1;
            }
        }
    }
    return formatted.length;
};

const groupInteger = (digits) => {
    // Strip leading zeros but keep a single zero meaningful entries handle later.
    const trimmed = digits.replace(/^0+(?=\d)/, "");
    return trimmed.replace(/\B(?=(\d{3})+(?!\d))/g, ".");
};

// Turn raw user text into a vi-VN formatted string.
const formatValue = (raw, decimalPlaces) => {
    if (!raw) {
        return "";
    }

    // Drop grouping dots first, then keep only digits and the first comma.
    let cleaned = raw.replace(/\./g, "");
    const hasComma = decimalPlaces > 0 && cleaned.includes(",");

    const commaIndex = cleaned.indexOf(",");
    let intPart = (hasComma ? cleaned.slice(0, commaIndex) : cleaned).replace(/\D/g, "");
    let decPart = hasComma ? cleaned.slice(commaIndex + 1).replace(/\D/g, "") : "";

    if (decimalPlaces > 0 && decPart.length > decimalPlaces) {
        decPart = decPart.slice(0, decimalPlaces);
    }

    const intFmt = groupInteger(intPart) || (hasComma ? "0" : "");

    return hasComma ? `${intFmt},${decPart}` : intFmt;
};

// Final formatting on blur: pad decimals so the value matches VnMoney.Format.
const formatOnBlur = (raw, decimalPlaces) => {
    const formatted = formatValue(raw, decimalPlaces);
    if (!formatted) {
        return "";
    }
    if (decimalPlaces <= 0) {
        return formatted;
    }

    const [intPart, decPart = ""] = formatted.split(",");
    const padded = (decPart + "0".repeat(decimalPlaces)).slice(0, decimalPlaces);
    return `${intPart || "0"},${padded}`;
};

// Convert a formatted string into an invariant numeric string for .NET parsing.
const toInvariant = (formatted) => formatted.replace(/\./g, "").replace(",", ".");

export function init(el, dotnet, options) {
    if (!el) {
        return;
    }

    const decimalPlaces = options?.decimalPlaces ?? 0;
    const state = { disposed: false };

    // A DOM event (blur fired right before a click that navigates/disposes the
    // component) can call back into .NET after the DotNetObjectReference is gone.
    // Skip when disposed and swallow the disconnect rejection so it never throws.
    const safeInvoke = (method, arg) => {
        if (state.disposed) {
            return;
        }
        dotnet.invokeMethodAsync(method, arg).catch(() => {});
    };

    const onInput = () => {
        const caret = el.selectionStart ?? el.value.length;
        const significantBeforeCaret = countSignificant(el.value.slice(0, caret));

        const formatted = formatValue(el.value, decimalPlaces);
        el.value = formatted;

        const newCaret = caretFromSignificant(formatted, significantBeforeCaret);
        try {
            el.setSelectionRange(newCaret, newCaret);
        } catch {
            // Some input types do not support selection; ignore.
        }

        safeInvoke("OnInput", toInvariant(formatted));
    };

    const onBlur = () => {
        const formatted = formatOnBlur(el.value, decimalPlaces);
        el.value = formatted;
        safeInvoke("OnBlur", toInvariant(formatted));
    };

    el.addEventListener("input", onInput);
    el.addEventListener("blur", onBlur);
    instances.set(el, { onInput, onBlur, state });
}

export function dispose(el) {
    const handlers = instances.get(el);
    if (handlers) {
        handlers.state.disposed = true;
        el.removeEventListener("input", handlers.onInput);
        el.removeEventListener("blur", handlers.onBlur);
        instances.delete(el);
    }
}
