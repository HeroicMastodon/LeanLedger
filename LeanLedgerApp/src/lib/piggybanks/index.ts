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
    closeDate?: string;
    targetBalance?: number;
    entries: PiggyBankEntry[];
}

export type PiggyBankEntry = {
    id: string;
    date: Date;
    amount: number;
    description: string;
    transaction?: {
        id: string;
        description: TransactionType;
    }
}

export function defaultPiggyBank(): PiggyBank {
    return {
        id: "",
        name: "",
        openDate: new Date(),
        entries: [],
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
