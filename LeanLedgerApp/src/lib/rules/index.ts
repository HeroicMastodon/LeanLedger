import {type SelectOption, splitPascal} from "$lib";

export type RuleTrigger = {
    field: RuleTransactionField;
    not: boolean;
    condition: RuleCondition;
    value?: string;
}
export type RuleAction = {
    actionType: RuleActionType;
    field?: RuleTransactionField;
    value?: string;
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
