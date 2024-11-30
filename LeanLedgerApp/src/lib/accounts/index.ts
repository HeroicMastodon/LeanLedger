import type {Transaction} from "$lib/transactions";

export type Account = {
    id: string;
    name: string;
    balance: number;
    balanceChange: number;
    active: boolean;
    lastActivityDate?: string;
}
export type AccountGrouping = Record<AccountType, Account[]>;
export const accountTypes = ["Bank", "CreditCard", "Merchant"] as const;
export type AccountType = typeof accountTypes[number];

export type AccountData = {
    id: string;
    name: string;
    accountType: AccountType;
    openingBalance: number;
    balance: number;
    openingDate: string;
    active: boolean;
    includeInNetWorth: boolean;
    notes: string;
    transactions: Transaction[];
}
