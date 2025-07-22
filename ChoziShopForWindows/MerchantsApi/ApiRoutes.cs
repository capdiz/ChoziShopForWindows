using ChoziShopForWindows.MerchantsApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ChoziShopForWindows.MerchantsApi
{
    public static class ApiRoutes
    {

        public static UrlBuilder RestartSessionUrl()
        {
            return new UrlBuilder(HttpService.BASE_URL)
                .AddPath("restart_merchant_session");
        }
        public static UrlBuilder WindowsSession()
        {
            return new UrlBuilder(HttpService.BASE_URL)
                .AddPath("windows_sessions")
                .AddPath("unknown")
                .AddPath("validate_session");
        }

        public static UrlBuilder ActivateWindowsSessionUrl(long sessionResponseId)
        {
            return new UrlBuilder(HttpService.BASE_URL)
                .AddPath("windows_sessions")
                .AddPath((int)sessionResponseId)
                .AddPath("activate_device");
        }

        public static UrlBuilder GetWindowsAccountStoresUrl()
        {
            return new UrlBuilder(HttpService.BASE_URL)
                .AddPath("windows_accounts")
                .AddPath(0)
                .AddPath("fetch_merchant_stores");
        }

        public static UrlBuilder GetCategorySectionsUrl()
        {
            return new UrlBuilder(HttpService.BASE_URL)
                .AddPath("windows_accounts")
                .AddPath(0)
                .AddPath("inventory_items");
        }
        
        public static UrlBuilder CreateKeeperAccount()
        {
            return new UrlBuilder(HttpService.BASE_URL)
                .AddPath("keepers");
        }

        public static UrlBuilder SyncUnSyncedOrderToApi(long onlineStoreId)
        {
            return new UrlBuilder(HttpService.BASE_URL)
                .AddPath("stores").AddPath((int)onlineStoreId)
                .AddPath("orders");
        }
        public static UrlBuilder CreateCategoryProduct(long onlineStoreId, long categorySectionId)
        {
            return new UrlBuilder(HttpService.BASE_URL)
                .AddPath("stores").AddPath((int)onlineStoreId)
                .AddPath("inventories").AddPath(0)
                .AddPath("category_sections").AddPath((int)categorySectionId)
                .AddPath("category_products");
        }

        public static UrlBuilder UpdateCategoryProduct(long onlineStoreId, long categorySectionId, long onlineCategoryProductId)
        {
            return new UrlBuilder(HttpService.BASE_URL)
            .AddPath("stores").AddPath((int)onlineStoreId)
            .AddPath("inventories").AddPath(0)
            .AddPath("category_sections").AddPath((int)categorySectionId)
            .AddPath("category_products").AddPath((int)onlineCategoryProductId);
        }

        public static UrlBuilder DeleteCategoryProduct(long onlineStoreId, long categorySectionId, long onlineCategoryProductId)
        {
            return new UrlBuilder(HttpService.BASE_URL)
                .AddPath("stores").AddPath((int)onlineStoreId)
                .AddPath("inventories").AddPath(0)
                .AddPath("CategorySections").AddPath((int)categorySectionId)
                .AddPath("CategoryProducts").AddPath((int)onlineCategoryProductId);

        }

        public static UrlBuilder SyncUnSyncedObjectToApi(long onlineStoreId)
        {
            return new UrlBuilder(HttpService.BASE_URL)
                .AddPath("stores").AddPath((int)onlineStoreId)
                .AddPath("sync_object");
        }

        public static UrlBuilder CreateOrderUrl(long onlineStoreId)
        {
            return new UrlBuilder(HttpService.BASE_URL)
                .AddPath("stores")
                .AddPath((int)onlineStoreId)
                .AddPath("orders");
        }

        public static UrlBuilder VerifyKeeper()
        {
            return new UrlBuilder(HttpService.BASE_URL)
                .AddPath("keepers")
                .AddPath(0)
                .AddPath("verify_code");
        }

        public static UrlBuilder FetchCategorySections()
        {
            return new UrlBuilder(HttpService.BASE_URL)
                .AddPath("categories");
        }

        public static UrlBuilder CreateCategorySection(long storeId)
        {
            return new UrlBuilder(HttpService.BASE_URL)
                .AddPath("stores")
                .AddPath((int)storeId)
                .AddPath("inventories")
                .AddPath(0)
                .AddPath("category_sections");
        }

        public static UrlBuilder DeleteCategorySection(long storeId, long categorySectionId)
        {
            return new UrlBuilder(HttpService.BASE_URL)
                .AddPath("stores")
                .AddPath((int)storeId)
                .AddPath("inventories")
                .AddPath(0)
                .AddPath("category_sections")
                .AddPath((int)categorySectionId);

        }

        public static UrlBuilder GetDefaultCategorySectionProducts(long onlineCategoryId)
        {
            return new UrlBuilder(HttpService.BASE_URL)
                .AddPath("categories").AddPath((int)onlineCategoryId);
        }
    }
}
