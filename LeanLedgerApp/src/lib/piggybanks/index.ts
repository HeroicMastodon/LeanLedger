import type { TransactionType } from "$lib/transactions";

export type Piggy = {
    id: string;
    name: string;
    initialBalance: number;
    balanceTarget?: number | null;
    balance: number;
    progressPercent?: number | null;
    closed: boolean;
}

export type PiggyData = {
    id: string;
    name: string;
    initialBalance: number;
    balanceTarget?: number;
    balance?: number;
}

export type PiggyAllocation = {
    id: string;
    transactionId: string;
    piggyBankId: string;
    amount: number;
}

export type PiggyAllocationData = PiggyAllocation & {
    transactionAmount: number;
    description: string;
    category: string;
    type: TransactionType;
    sourceAccountId: string;
    sourceAccountName: string;
}

export type PiggyWithAllocationData = PiggyData & {
    allocations: PiggyAllocationData[];
}

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
    transaction?: {
        id: string;
        description: TransactionType;
    }
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

export function defaultPiggyData(): PiggyData {
    return {
        id: "",
        name: "",
        initialBalance: 0,
        balanceTarget: undefined,
    };
}
