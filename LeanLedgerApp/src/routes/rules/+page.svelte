<script lang="ts">
    import {apiClient} from "$lib/apiClient";
    import {ProgressBar} from "@skeletonlabs/skeleton";
    import {actionToString, defaultRule, type Rule, type RuleGroup, triggerToString} from "$lib/rules";
    import SimpleExpandingList from "$lib/components/SimpleExpandingList.svelte";
    import {Dialog} from "$lib/dialog.svelte";
    import DefaultDialog from "$lib/components/dialog/DefaultDialog.svelte";
    import LabeledInput from "$lib/components/forms/LabeledInput.svelte";
    import FormButton from "$lib/components/dialog/FormButton.svelte";
    import RunRuleButton from "$lib/rules/RunRuleButton.svelte";
    import DeleteConfirmationButton from "$lib/components/dialog/DeleteConfirmationButton.svelte";
    import {goto} from "$app/navigation";
    import type {SelectOption} from "$lib";
    import {loadAccountOptions} from "$lib/transactions";

    let accounts = $state<SelectOption<string>[]>([]);
    let ruleGroups = $state(load());
    function accountNameFromId(id?: string) {
        return accounts.find(a => a.value === id)?.display;
    }

    async function load() {
        accounts = await loadAccountOptions();
        let res = await apiClient.get<RuleGroup[]>("rule-groups");
        return res.data;
    }

    let ruleGroupDialog = new Dialog();
    let ruleGroupUpdate = $state<{ previous?: string; current: string; }>({
        previous: undefined,
        current: ""
    })
    const ruleGroupDialogHeader = $derived(ruleGroupUpdate.previous ? "Update" : "New");

    function openRuleGroupDialog(previousValue?: string) {
        ruleGroupUpdate.previous = previousValue;
        ruleGroupUpdate.current = previousValue ?? "";
        ruleGroupDialog.open();
    }

    async function saveRuleGroup() {
        console.log($state.snapshot(ruleGroupUpdate))
        const payload = {name: ruleGroupUpdate.current.trim()};
        if (ruleGroupUpdate.previous) {
            const res = await apiClient.put(`rule-groups/${ruleGroupUpdate.previous}`, payload);
        } else {
            const res = await apiClient.post(`rule-groups`, payload);
        }

        ruleGroups = load();
    }

    let changedCount: Promise<number | undefined> = $state(Promise.resolve(undefined));

    function runAllRules(startDate: string, endDate: string) {
        changedCount = apiClient
            .post<{ count: number }>("rules/run-all", {startDate, endDate})
            .then(res => res.data.count);
    }

    function runRuleGroup(name: string | undefined, startDate: string, endDate: string) {
        changedCount = apiClient
            .post<{ count: number }>(`rule-groups/run`, {startDate, endDate, ruleGroupName: name})
            .then(res => res.data.count);
    }

    async function deleteRuleGroup(name: string) {
        await apiClient.delete(`rule-groups/${name}`);
        ruleGroups = load();
        return true;
    }

    let newRuleName = $state("");

    async function createNewRule(ruleGroupName?: string) {
        const res = await apiClient.post<Rule>(`rules`, {
            ...defaultRule(),
            ruleGroupName,
            name: newRuleName
        })

        await goto(`/rules/${res.data.id}`);
        return true;
    }
</script>

<div class="mb-8 flex gap-4 items-center">
    <h1 class="h1">Rules</h1>
    <button onclick={() => openRuleGroupDialog()} class="btn variant-filled-secondary">New Group</button>
    <RunRuleButton
        text="Run all rules"
        countPromise={changedCount}
        run={runAllRules}
    />
</div>

{#await ruleGroups}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:then groups}
    {#each groups as group}
        <div class="card mb-8 p-4">
            <div class="flex justify-between mb-4">
                <div class="flex gap-4">
                    <h2 class="h2">{group.name || "(ungrouped)"}</h2>
                    {#if group.name}
                        <button
                            class="btn variant-outline-tertiary"
                            onclick={() => openRuleGroupDialog(group.name)}
                        >Edit
                        </button>
                        <DeleteConfirmationButton
                            onDelete={() => deleteRuleGroup(group.name ?? "")}
                        />
                    {/if}
                    <RunRuleButton
                        text="Run Rule Group"
                        countPromise={changedCount}
                        run={(start, end) => runRuleGroup(group.name, start, end)}
                    />
                </div>
                <FormButton
                    class="variant-outline-secondary"
                    text="New Rule"
                    confirmText="Save"
                    onConfirm={() => createNewRule(group.name)}
                >
                    <LabeledInput
                        label="Name"
                        type="text"
                        bind:value={newRuleName}
                    />
                </FormButton>
            </div>
            <div class="table-container">
                <table class="table table-compact table-hover table-fixed">
                    <thead>
                    <tr>
                        <th> Name</th>
                        <th>Triggers when</th>
                        <th>Rule will</th>
                    </tr>
                    </thead>
                    <tbody>
                    {#each group.rules as rule}
                        <tr>
                            <td>
                                <a class="text-primary-400" href="rules/{rule.id}">
                                    {rule.name}
                                </a>
                            </td>
                            <td>
                                <SimpleExpandingList
                                    items={rule.triggers}
                                    stringify={(trigger) => triggerToString(trigger, accountNameFromId(trigger.value))}
                                    label="Triggers"
                                />
                            </td>
                            <td>
                                <SimpleExpandingList
                                    items={rule.actions}
                                    stringify={actionToString}
                                    label="Actions"
                                />
                            </td>
                        </tr>
                    {/each}
                    </tbody>
                </table>
            </div>
        </div>
    {/each}
    <DefaultDialog bind:dialog={ruleGroupDialog.value} onenter={saveRuleGroup}>
        <div class="flex flex-col gap-8 items-center">
            <h2 class="h2">{ruleGroupDialogHeader} Rule Group</h2>
            <LabeledInput
                type="text"
                bind:value={ruleGroupUpdate.current}
                label="Name"
            />
            <div class="flex flex-row gap-8">
                <button onclick={() => ruleGroupDialog.close()} class="btn variant-outline-error">Cancel</button>
                <button onclick={saveRuleGroup} class="btn variant-filled-success">Save</button>
            </div>
        </div>
    </DefaultDialog>
{/await}
