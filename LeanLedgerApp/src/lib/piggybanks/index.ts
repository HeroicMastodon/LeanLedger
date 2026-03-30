
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
}

export function defaultPiggyData(): PiggyData {
    return {
        id: "",
        name: "",
        initialBalance: 0,
        balanceTarget: undefined,
    };
}
