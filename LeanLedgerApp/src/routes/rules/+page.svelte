<script lang="ts">
    import {apiClient} from "$lib/apiClient";
    import {ProgressBar} from "@skeletonlabs/skeleton";
    import {actionToString, type RuleGroup, triggerToString} from "$lib/rules";
    import SimpleExpandingList from "$lib/components/SimpleExpandingList.svelte";

    let ruleGroups = $state(load())

    async function load() {
        let res = await apiClient.get<RuleGroup[]>("rule-groups");
        return res.data;
    }

    // TODO: Implement the following dialogs
    // ! Can use the DefaultDialog Component for these
    let ruleDialog: HTMLDialogElement | undefined = $state();
    let ruleGroupDialog: HTMLDialogElement | undefined = $state();
    let deleteConfirmationDialog: HTMLDialogElement | undefined = $state();
</script>

<div class="mb-8 flex gap-4 items-center">
    <h1 class="h1">Rules</h1>
    <button class="btn variant-filled-secondary">New Group</button>
</div>

{#await ruleGroups}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:then groups}
    {#each groups as group}
        <div class="card mb-8 p-4">
            <div class="flex justify-between mb-4">
                <div class="flex gap-4">
                    <h2 class="h2">{group.name}</h2>
                    <button class="btn variant-outline-tertiary">Edit</button>
                </div>
                <button class="btn variant-outline-secondary">New Rule</button>
            </div>
            <div class="table-container">
                <table class="table table-fixed">
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
                                    stringify={triggerToString}
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
{/await}
