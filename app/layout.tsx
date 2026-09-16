import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "GameChanger | Build your next cycle",
  description: "Create focused improvement cycles and build a consistent weekly review rhythm.",
  icons: {
    icon: "/favicon.svg",
    shortcut: "/favicon.svg",
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body className="antialiased">{children}</body>
    </html>
  );
}
