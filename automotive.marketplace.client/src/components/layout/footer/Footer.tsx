import { PiGithubLogo, PiLinkedinLogo } from "react-icons/pi";

export default function Footer() {
  return (
    <footer className="border-border mt-auto border-t">
      <div className="mx-8 flex items-center justify-between py-6 xl:mx-auto xl:max-w-6xl">
        <div className="flex items-center gap-6">
          <a
            href="https://github.com/taurasbear/automotive-marketplace"
            target="_blank"
            rel="noopener noreferrer"
            className="text-muted-foreground hover:text-foreground flex items-center gap-2 text-sm transition-colors"
          >
            <PiGithubLogo className="h-5 w-5" />
            <span>GitHub</span>
          </a>
          <a
            href="https://www.linkedin.com/in/tauras-narvilas/"
            target="_blank"
            rel="noopener noreferrer"
            className="text-muted-foreground hover:text-foreground flex items-center gap-2 text-sm transition-colors"
          >
            <PiLinkedinLogo className="h-5 w-5" />
            <span>LinkedIn</span>
          </a>
        </div>
      </div>
    </footer>
  );
}
