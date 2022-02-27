﻿using System;
using System.Threading.Tasks;
using Auth0.Core.Exceptions;
using Auth0.ManagementApi.IntegrationTests.Testing;
using Auth0.Tests.Shared;
using Xunit;

namespace Auth0.ManagementApi.IntegrationTests
{
    public class BrandingClientTests : ManagementTestBase, IAsyncLifetime
    {
        public async Task InitializeAsync()
        {
            string token = await GenerateManagementApiToken();

            ApiClient = new ManagementApiClient(token, GetVariable("AUTH0_MANAGEMENT_API_URL"), new HttpClientManagementConnection(options: new HttpClientManagementConnectionOptions { NumberOfHttpRetries = 9 }));
        }

        [Fact]
        public async Task Test_Branding_Reand_And_Update()
        {
            try
            {
                // Update branding
                await ApiClient.Branding.UpdateAsync(new Models.BrandingUpdateRequest
                {
                    LogoUrl = "https://cdn.auth0.com/website/press/resources/auth0-logo-monotone-white.svg",
                    FaviconUrl = "https://cdn.auth0.com/website/press/resources/auth0-logo-monotone-black.svg",
                    Colors = new Models.BrandingColors
                    {
                        PageBackground = "#ffffff",
                        Primary = "#ff0000"
                    },
                    Font = new Models.BrandingFont
                    {
                        Url = "https://fonts.googleapis.com/css2?family=Open+Sans&display=swap"
                    }
                });

                var updatedBranding = await ApiClient.Branding.GetAsync();

                Assert.Equal("https://cdn.auth0.com/website/press/resources/auth0-logo-monotone-white.svg", updatedBranding.LogoUrl);
                Assert.Equal("https://cdn.auth0.com/website/press/resources/auth0-logo-monotone-black.svg", updatedBranding.FaviconUrl);
                Assert.Equal("https://fonts.googleapis.com/css2?family=Open+Sans&display=swap", updatedBranding.Font.Url);
                Assert.Equal("#ff0000", updatedBranding.Colors.Primary);
                Assert.Equal("#ffffff", updatedBranding.Colors.PageBackground);

            }
            finally
            {
                // Rollback
                await ApiClient.Branding.UpdateAsync(new Models.BrandingUpdateRequest
                {
                    Colors = new Models.BrandingColors { 
                        Primary = "#0059d6",
                        PageBackground = "#000000"
                    },
                    LogoUrl = "https://cdn.auth0.com/website/press/resources/auth0-logo-monotone-black.svg",
                    FaviconUrl = "https://cdn2.auth0.com/styleguide/latest/lib/logos/img/favicon.png",
                    Font = new Models.BrandingFont
                    {
                        Url = "https://fonts.googleapis.com/css2?family=Roboto&display=swap"
                    },
                });
            }
        }

        [Fact(Skip = "Enable when migrated to new Tenant because missing permissions on current")]
        public async Task Test_Branding_ULPTemplate_Reand_Update_Delete()
        {
            // var temp = await ApiClient.Branding.GetUniversalLoginTemplateAsync();
            try
            {
                var newTemplateBody = @"<!DOCTYPE html><html>
                  <head>
                    {%- auth0:head -%}
                  </head>
                  <body>
                    {%- auth0:widget -%}
                  </body></html> ";

                await ApiClient.Branding.SetUniversalLoginTemplateAsync(new Models.UniversalLoginTemplateUpdateRequest
                {
                    Template = newTemplateBody
                });

                var updatedTemplate = await ApiClient.Branding.GetUniversalLoginTemplateAsync();

                Assert.Equal(newTemplateBody, updatedTemplate.Body);
            }
            finally
            {
                await ApiClient.Branding.DeleteUniversalLoginTemplateAsync();

                await Assert.ThrowsAsync<ErrorApiException>(() => ApiClient.Branding.GetUniversalLoginTemplateAsync());
            }
        }
    }
}