import { splitPascal } from "$lib";
import { apiClient } from "$lib/apiClient";

export type RuleTrigger = {
    field: RuleTransactionField;
    not: boolean;
    condition: RuleCondition;
    value?: string;
}

export function defaultRuleTrigger(): RuleTrigger {
    return {
        field: "Description",
        not: false,
        condition: "StartsWith",
    }
}

export type RuleAction = {
    actionType: "Set" | "Append";
    field: RuleTransactionField;
    // Easier to allow numbers since svelte inputs auto convert
    // But this is frontend only and we convert to string before sending to backend
    value: string | number;
} | {
    actionType: "Clear";
    field: RuleTransactionField;
} | {
    actionType: "DeleteTransaction";
} | {
    actionType: "CreatePiggyEntry";
    piggyBankId: string;
    amount: number;
    description: string;
    piggyBankName?: string;
}
// Create an array of rule action types from the RuleActionV2 type, not select options
export type RuleActionType = RuleAction["actionType"];
export const ruleActionTypes: RuleActionType[] = [
    "Set",
    "Append",
    "Clear",
    "DeleteTransaction",
    "CreatePiggyEntry",
] as const;


export function defaultRuleAction(): RuleAction {
    return {
        actionType: "Set",
        field: "Description",
        value: "",
    }
}

export const ruleTransactionFields = [
    "Description",
    "Date",
    "Amount",
    "Type",
    "Category",
    "Source",
    "Destination"
] as const;
export type RuleTransactionField = typeof ruleTransactionFields[number];
export const ruleConditions = [
    "StartsWith",
    "EndsWith",
    "Contains",
    "IsExactly",
    "GreaterThan",
    "LessThan",
    "Exists",
] as const;
export type RuleCondition = typeof ruleConditions[number];
export type RuleGroup = {
    name?: string;
    rules: Rule[];
}
export type Rule = {
    id: string;
    name: string;
    isStrict: boolean;
    triggers: RuleTrigger[];
    actions: RuleAction[];
    ruleGroupName?: string;
}

export function defaultRule(): Rule {
    return {
        id: "",
        name: "",
        isStrict: true,
        triggers: [],
        actions: [],
    }
}

export function triggerToString(trigger: RuleTrigger, accountName?: string) {
    return `When ${splitPascal(trigger.field)} ${trigger.not ? "Not " : ""}${splitPascal(trigger.condition)} ${accountName || trigger.value || ""}`
}
export function isAccountField(field?: RuleTransactionField) {
    return field === "Source" || field === "Destination";
}


export function debounce(callback: Function, wait = 300) {
    let timeout: ReturnType<typeof setTimeout>;

    return (...args: any[]) => {
        clearTimeout(timeout);
        timeout = setTimeout(() => callback(...args), wait);
    };
}

export async function loadCompletionsForField(
    field: RuleTransactionField,
    value?: string,
    condition?: RuleCondition,
): Promise<string[]> {
    const isAction = condition === undefined;
    if (!value || (isAction && field === "Description")) {
        return [];
    }

    if (field === "Description") {
        const res = await apiClient.get<string[]>(
            `Completions/descriptions?value=${value}&condition=${condition}`,
        );
        return res.data;
    } else if (field === "Category") {
        const res = await apiClient.get<string[]>(
            `Completions/categories?value=${value}&condition=${condition}`,
        );
        return res.data;
    }

    return [];
}
