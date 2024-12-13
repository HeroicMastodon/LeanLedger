import type {Transaction} from "$lib/transactions";
import type {SelectOption} from "$lib";

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
export const accountTypeOptions: SelectOption<AccountType>[] = accountTypes.map(a => ({display: a, value: a}));
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

export function defaultAccountData(): AccountData {
    return {
        id: "",
        name: "",
        accountType: "Bank",
        openingBalance: 0,
        balance: 0,
        openingDate: "2024-01-01",
        active: true,
        includeInNetWorth: true,
        notes: "",
        transactions: [],
    };
}
