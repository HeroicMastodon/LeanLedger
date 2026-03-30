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

export function defaultPiggyData(): PiggyData {
    return {
        id: "",
        name: "",
        initialBalance: 0,
        balanceTarget: undefined,
    };
}
