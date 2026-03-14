from playwright.sync_api import sync_playwright

with sync_playwright() as p:
    browser = p.chromium.launch()
    page = browser.new_page()
    
    def handle_request(request):
        if "tiles" in request.url or ("png" in request.url and "map" in request.url):
            print("Found tile URL:", request.url)
            
    page.on("request", handle_request)
    page.goto("https://albionfreemarket.com/albion-map", wait_until="networkidle")
    browser.close()
