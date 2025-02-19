<script lang="ts">
    import '../app.postcss';

    // Floating UI for Popups
    import {arrow, autoUpdate, computePosition, flip, offset, shift} from '@floating-ui/dom';
    import {
        AppBar,
        AppShell,
        Drawer,
        getDrawerStore,
        initializeStores,
        popup,
        storePopup
    } from '@skeletonlabs/skeleton';
    import {Fa} from "svelte-fa";
    import {faCaretDown, faDollar, faPalette,} from "@fortawesome/free-solid-svg-icons";

    import {page} from "$app/stores";

    import {monthManager} from "$lib/selectedMonth.svelte";
    import {afterNavigate} from "$app/navigation";
    import {faArrowLeft} from "@fortawesome/free-solid-svg-icons/faArrowLeft";
    import {faArrowRight} from "@fortawesome/free-solid-svg-icons/faArrowRight";
    import {faHamburger} from "@fortawesome/free-solid-svg-icons/faHamburger";


    interface Props {
        children?: import('svelte').Snippet;

        [key: string]: any
    }

    let props: Props = $props();
    initializeStores();
    storePopup.set({computePosition, autoUpdate, flip, shift, offset, arrow});

    const themes = ['carbon-fox', 'wintry'] as const;
    let selectedTheme = $state(window.localStorage.getItem("theme") ?? 'carbon-fox');

    $effect(() => {
        window.localStorage.setItem("theme", selectedTheme);
        document.body.setAttribute('data-theme', selectedTheme);
    })

    const drawerStore = getDrawerStore();

    afterNavigate(() => {
        monthManager.selectFromQuery($page.url.searchParams)
        drawerStore.close();
    })

    function isActive(path: string, href: string) {
        const pathWithoutLeadingSlash = path.substring(1);
        const hrefWithoutQuery = href.split("?")[0];
        const firstInHrefPath = hrefWithoutQuery.split("/")[1];

        if (firstInHrefPath === "") {
            return pathWithoutLeadingSlash === "";
        }

        return pathWithoutLeadingSlash.startsWith(firstInHrefPath)
    }

    const isControlledByGlobalMonth = $derived.by(() => {
        const path = $page.url.pathname.toLowerCase();

        return !path.startsWith("/rules");
    })
    const selectedMonthParams = $derived(`?${monthManager.params}`)
</script>
{#snippet navItem(href: string, title: string)}
    <li>
        <a href="{href}"
           class:bg-primary-active-token={isActive($page.url.pathname, href)}
           data-sveltekit-preload-data="hover"
        >
            <span class="flex-auto">{title}</span>
        </a>
    </li>
{/snippet}
{#snippet navigation(renderHome: boolean)}
    <div class="h-full bg-surface-50-900-token border-r border-surface-500/30 {props.class ?? ''}">
        <section class="p-4 pb-20 space-y-4 overflow-y-auto">
            <nav class="list-nav">
                <ul>
                    {#if renderHome}
                        {@render navItem(`/${selectedMonthParams}`, "Home")}
                    {/if}
                    {@render navItem(`/accounts${selectedMonthParams}`, "Accounts")}
                    {@render navItem(`/transactions${selectedMonthParams}`, "Transactions")}
                    {@render navItem(`/categories${selectedMonthParams}`, "Categories")}
                    {@render navItem("/rules", "Rules")}
                    {@render navItem(`/budgets${selectedMonthParams}`, "Budgets")}
                </ul>
            </nav>
        </section>
    </div>

{/snippet}

<Drawer>
    <h1 class="h1 m-4">Lean Ledger</h1>
    <hr class="hr"/>
    {@render navigation(true)}
</Drawer>
<AppShell slotSidebarLeft="w-0 lg:w-64">
    {#snippet header()}

        <!-- App Bar -->
        <AppBar>
            {#snippet lead()}
                <a aria-label="Home"
                   class="lg:btn-icon lg:btn-icon-xl w-0 overflow-hidden lg:overflow-visible"
                   href="/{selectedMonthParams}"
                >
                    <Fa icon={faDollar} />
                </a>
                <button
                    onclick={() => drawerStore.open({})}
                    class="btn-icon btn-icon-xl w-fit overflow-visible lg:w-0 lg:overflow-hidden"
                >
                    <Fa icon={faHamburger} />
                </button>
            {/snippet}
            <a href="/{selectedMonthParams}"
               class="h2 font-bold w-0 hidden overflow-hidden lg:w-fit lg:overflow-visible lg:inline"
            >
                Lean Ledger
            </a>
            {#snippet trail()}
                {#if isControlledByGlobalMonth}
                    <div class="flex items-center">
                        <p class="p">{monthManager.selectedMonth.name} {monthManager.selectedMonth.year}</p>
                        <a href="{$page.url.pathname}?month={monthManager.lastMonth.number}&year={monthManager.lastMonth.year}"
                           class="btn btn-icon-sm text-tertiary-500 p-1"
                        >
                            <Fa icon={faArrowLeft} />
                        </a>
                        <a href="{$page.url.pathname}?year={monthManager.nextMonth.year}&month={monthManager.nextMonth.number}"
                           class="btn btn-icon-sm text-tertiary-500 p-1"
                        >
                            <Fa icon={faArrowRight} />
                        </a>
                    </div>
                {/if}
                <button class="btn hover:variant-soft-primary"
                        use:popup={{ event: 'click', target: 'theme', closeQuery: 'a[href]' }}
                >
                    <Fa icon={faPalette} class="svelte-fa-size-lg md:!hidden"></Fa>
                    <span class="hidden md:inline-block">Theme</span>
                    <Fa icon={faCaretDown} class="fa-solid opacity-50"></Fa>
                </button>
                <div class="card p-4 w-60 shadow-xl" data-popup="theme">
                    <div class="flex flex-col gap-4">
                        {#each themes as theme}
                            <button
                                class="btn"
                                class:bg-primary-active-token={selectedTheme === theme}
                                onclick={() => {
                                selectedTheme = theme;
                            }}
                            >{theme}</button>
                        {/each}
                    </div>
                </div>
            {/snippet}
        </AppBar>

    {/snippet}
    {#snippet sidebarLeft()}
        <!-- Nav List -->
        {@render navigation(false)}
    {/snippet}
    <!-- Page Route Content -->
    <section class="m-8">
        {@render props.children?.()}
    </section>

</AppShell>
