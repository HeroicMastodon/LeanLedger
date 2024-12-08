export type Transaction = {
    id: string;
    description: string;
    amount: number;
    date: string;
    sourceAccount?: AttachedAccount;
    destinationAccount?: AttachedAccount;
    category?: string;
}
export type AttachedAccount = {
    id: string;
    name: string;
}

export function defaultTransaction(): Transaction {
    return {
        id: "",
        description: "",
        amount: 0,
        category: "",
        date: "",
    }
}
