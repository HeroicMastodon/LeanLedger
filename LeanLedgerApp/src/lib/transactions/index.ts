import type {SelectOption} from "$lib";
import {apiClient} from "$lib/apiClient";
import {todaysDateString} from "$lib/dateTools";

export type Transaction = {
    id: string;
    description: string;
    amount: number;
    date: string;
    sourceAccount?: AttachedAccount;
    destinationAccount?: AttachedAccount;
    category?: string;
    type: TransactionType;
}
export type EditableTransaction = Omit<Transaction, 'sourceAccount' | 'destinationAccount'> & {
    sourceAccountId?: string;
    destinationAccountId?: string;
}
export type AttachedAccount = {
    id: string;
    name: string;
}

export type TransactionType = 'Income' | 'Expense' | 'Transfer';

export const transactionTypeOptions: SelectOption<TransactionType>[] = [
    {
        display: "Deposit",
        value: "Income",
    },
    {
        display: "Withdrawal",
        value: "Expense",
    },
    {
        display: "Transfer",
        value: "Transfer",
    },
]

export function defaultTransaction(): EditableTransaction {
    return {
        id: "",
        description: "",
        amount: 0,
        category: "",
        date: todaysDateString(),
        type: "Income"
    }
}

export async function loadCategoryOptions(): Promise<string[]> {
    const res = await apiClient.get<string[]>("categories/options");
    return res.data;
}

export async function loadAccountOptions(): Promise<SelectOption<string>[]> {
    const res = await apiClient.get<AttachedAccount[]>("accounts/options");
    return res.data.map(a => ({value: a.id, display: a.name}));
}
