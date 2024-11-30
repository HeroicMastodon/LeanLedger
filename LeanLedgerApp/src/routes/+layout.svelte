<script lang="ts">
	import '../app.postcss';

	// Floating UI for Popups
	import { computePosition, autoUpdate, flip, shift, offset, arrow } from '@floating-ui/dom';
    import {AppBar, AppShell, storePopup} from '@skeletonlabs/skeleton';
    import {Fa} from "svelte-fa";
    import {faDollar} from "@fortawesome/free-solid-svg-icons";

    import {page} from "$app/stores";

    interface Props {
		children?: import('svelte').Snippet;
        [key: string]: any
	}

	let  props: Props = $props();
	storePopup.set({ computePosition, autoUpdate, flip, shift, offset, arrow });
</script>
{#snippet navItem(href: string, title: string)}
    <li>
        <a href="{href}" class:bg-primary-active-token={href === $page.url.pathname} data-sveltekit-preload-data="hover">
            <span class="flex-auto">{title}</span>
        </a>
    </li>
{/snippet}

<AppShell>
    {#snippet header()}

        <!-- App Bar -->
        <AppBar>
            {#snippet lead()}
            <a aria-label="Home" class="btn-icon btn-icon-xl" href="/" >
                    <Fa icon={faDollar} />
                </a>
            {/snippet}
            <a href="/" class="h2 font-bold">
                Lean Ledger
            </a>
        </AppBar>

    {/snippet}
    {#snippet sidebarLeft()}

        <!-- Nav List -->
        <div class="h-full bg-surface-50-900-token border-r border-surface-500/30 {props.class ?? ''}">
            <section class="p-4 pb-20 space-y-4 overflow-y-auto">
                <nav class="list-nav">
                    <ul>
                        {@render navItem("/accounts", "Accounts")}
                    </ul>
                </nav>
            </section>
        </div>

    {/snippet}
    <!-- Page Route Content -->
    <section class="m-8">
        {@render props.children?.()}
    </section>

</AppShell>
