using Blazored.LocalStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mips.App
{
    public class TabManager
    {
        public List<Tab> Tabs { get; set; } = new();
        public int CurrentTabIndex = -1;
        public Tab CurrentTab
        {
            get
            {
                if (CurrentTabIndex >= 0 && CurrentTabIndex < Tabs.Count)
                {
                    return Tabs[CurrentTabIndex];
                }
                return null;
            }
        }

        private readonly ILocalStorageService localStorage;

        public TabManager(ILocalStorageService localStorage)
        {
            this.localStorage = localStorage;
        }

        public async Task InitializeAsync()
        {
            Tabs = await localStorage.GetItemAsync<List<Tab>>("tabs") ?? new();
            CurrentTabIndex = await localStorage.GetItemAsync<int?>("current") ?? -1;

            if (Tabs.Count == 0)
            {
                await AddTabAsync();
            }
            else if (CurrentTabIndex == -1 || CurrentTabIndex >= Tabs.Count)
            {
                await SelectTabAsync(0);
            }
        }

        public async Task AddTabAsync()
        {
            var id = NewId();
            Tabs.Add(new Tab(id, $"Tab {Tabs.Count + 1}"));
            CurrentTabIndex = Tabs.Count - 1;

            await localStorage.SetItemAsync("tabs", Tabs);
            await localStorage.SetItemAsync("current", CurrentTabIndex);
        }

        public async Task SelectTabAsync(int index)
        {
            if (index >= 0 && index < Tabs.Count)
            {
                CurrentTabIndex = index;
                await localStorage.SetItemAsync("current", CurrentTabIndex);
            }
        }

        public async Task CloseTabAsync(int index)
        {
            if (index >= 0 && index < Tabs.Count)
            {
                Tabs.RemoveAt(index);
                await localStorage.SetItemAsync("tabs", Tabs);
            }
        }

        private string NewId()
        {
            return Guid.NewGuid().ToString();
        }
    }

    public record Tab(string Id, string Title);
}
