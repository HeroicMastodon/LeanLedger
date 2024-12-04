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
