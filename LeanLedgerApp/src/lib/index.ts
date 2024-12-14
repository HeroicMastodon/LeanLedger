// place files you want to import through the `$lib` alias in this folder.
import type {TransactionType} from "$lib/transactions";

export function splitPascal(words: string) {
    return words.replace(/([a-z])([A-Z])/g, '$1 $2')
}

export function formatMoney(amount: number, type?: TransactionType) {
    if (type) {
        return type !== "Expense" ? `$${amount}` : `-$${amount}`;
    }

    return amount >= 0 ? `$${amount}` : `-$${amount * -1}`;
}

export function dateFromString(date: string) {
    return new Date(date).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    })
}

export type SelectOption<T> = {
    value: T;
    display: string;
}
