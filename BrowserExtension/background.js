let lastHero = "";

async function checkAndNavigate() {
  const apiUrl = "http://localhost:5005/hero";
  const baseUrl = "https://dota2protracker.com/hero/";

  try {
    const response = await fetch(apiUrl);

    if (response.ok) {
      const heroName = (await response.text()).trim();

      if (heroName && heroName !== lastHero) {
        lastHero = heroName;
        const targetUrl = `${baseUrl}${heroName}`;

        const tabs = await browser.tabs.query({
          url: "*://dota2protracker.com/hero/*",
        });

        if (tabs.length > 0) {
          await browser.tabs.update(tabs[0].id, {
            url: targetUrl,
            active: true,
          });
          console.log("Updated existing tab:", tabs[0].id);
        } else {
          await browser.tabs.create({
            url: targetUrl,
            active: true,
          });
          console.log("Opened new tab");
        }
      }
    }
  } catch (error) {
    console.error("Polling error:", error);
  }
}

setInterval(checkAndNavigate, 15000);
