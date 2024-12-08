import type {SelectOption} from "$lib";

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
export type AttachedAccount = {
    id: string;
    name: string;
}

export type TransactionType = 'Deposit' | 'Withdrawal' | 'Transfer';

export const TransactionTypeOptions: SelectOption<TransactionType>[] = [
    {
        display: "Deposit",
        value: "Deposit",
    },
    {
        display: "Withdrawal",
        value: "Withdrawal",
    },
    {
        display: "Transfer",
        value: "Transfer",
    },
]

export function defaultTransaction(): Transaction {
    return {
        id: "",
        description: "",
        amount: 0,
        category: "",
        date: "2024-01-01",
        type: "Deposit"
    }
}
