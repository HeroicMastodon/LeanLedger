// import type { TransactionType } from "$lib/transactions";

export type PiggyBank = {
    id: string;
    name: string;
    openDate: string;
    balance: number;
    closeDate?: string;
    targetBalance?: number;
    progress?: number;
}

export type PiggyBankEntry = {
    id: string;
    date: string;
    amount: number;
    description: string;
    piggyBank: {
        id: string;
        name: string;
    };
    // transaction?: {
    //     id: string;
    //     description: TransactionType;
    // }
}

export type PiggyBankWithEntries = PiggyBank & {
    entries: PiggyBankEntry[];
}

export function newDateString(): string {
    return new Date().toISOString().split("T")[0];
}

export function defaultPiggyBank(): PiggyBank {
    return {
        id: "",
        name: "",
        openDate: newDateString(),
        balance: 0,
    }
}

export function defaultPiggyBankEntry(): PiggyBankEntry {
    return {
        id: "",
        date: newDateString(),
        amount: 0,
        description: "",
        piggyBank: {
            id: "",
            name: "",
        },
    }
}

