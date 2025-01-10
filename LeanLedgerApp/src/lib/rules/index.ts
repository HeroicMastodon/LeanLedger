import {type SelectOption, splitPascal} from "$lib";

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
    actionType: RuleActionType;
    field?: RuleTransactionField;
    value?: string;
}

export function defaultRuleAction(): RuleAction {
    return {
        actionType: "Set",
        field: "Description",
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
export const ruleActionTypes = [
    "Append",
    "Set",
    "Clear",
    "DeleteTransaction"
] as const;
export type RuleActionType = typeof ruleActionTypes[number];
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

export function triggerToString(trigger: RuleTrigger) {
    return `When ${splitPascal(trigger.field)} ${trigger.not ? "Not " : ""}${splitPascal(trigger.condition)} ${trigger.value || ""}`
}

export function actionToString(action: RuleAction) {
    let result = splitPascal(action.actionType);

    if (action.field) {
        result += ` ${action.field} to ${action.value}`
    }

    return result;
}

export function debounce(callback: Function, wait = 300) {
    let timeout: ReturnType<typeof setTimeout>;

    return (...args: any[]) => {
        clearTimeout(timeout);
        timeout = setTimeout(() => callback(...args), wait);
    };
}
