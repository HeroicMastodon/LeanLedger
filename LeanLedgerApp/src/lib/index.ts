// place files you want to import through the `$lib` alias in this folder.
import type {TransactionType} from "$lib/transactions";

export function splitPascal(words: string) {
    return words.replace(/([a-z])([A-Z])/g, '$1 $2')
}

const formatCurrency = new Intl.NumberFormat("en-us", {style: "currency", currency: "USD"}).format;
export function formatMoney(amount: number, type?: TransactionType) {

    return type !== "Expense" ? formatCurrency(amount) : formatCurrency(-amount);
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

export type MaybePromise<T> = Promise<T> | T;


export function sumUp<T>(arr: T[], getter: (val: T) => number): number {
    return arr.reduce((agg, curr) => agg + getter(curr), 0);
}
