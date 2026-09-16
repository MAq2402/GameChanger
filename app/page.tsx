"use client";

import { FormEvent, useState } from "react";
import {
  AlertCircle,
  ArrowRight,
  CalendarDays,
  Check,
  CircleCheck,
  Layers3,
  LoaderCircle,
  RotateCcw,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

const apiBaseUrl = (
  process.env.NEXT_PUBLIC_GAMECHANGER_API_URL ??
  (process.env.NODE_ENV === "development" ? "http://localhost:5080" : "")
).replace(/\/$/, "");

const setupSteps = [
  {
    number: "01",
    title: "Shape the cycle",
    detail: "Choose a clear name, start date, and a pace you can sustain.",
    icon: CalendarDays,
  },
  {
    number: "02",
    title: "Add your focus areas",
    detail: "Group the goals that matter into a few useful categories.",
    icon: Layers3,
  },
  {
    number: "03",
    title: "Review and begin",
    detail: "Check the plan, then start when you are ready for week one.",
    icon: CircleCheck,
  },
];

type CycleDraft = {
  id: string;
  name: string;
  startDate: string;
  timeZoneId: string;
  lengthInWeeks: number;
  status: string;
  createdAtUtc: string;
};

type ValidationProblem = {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
};

function formatDate(date: string) {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeZone: "UTC",
  }).format(new Date(`${date}T00:00:00Z`));
}

function calculateEndDate(startDate: string, lengthInWeeks: number) {
  const endDate = new Date(`${startDate}T00:00:00Z`);
  endDate.setUTCDate(endDate.getUTCDate() + lengthInWeeks * 7 - 1);
  return endDate.toISOString().slice(0, 10);
}

export default function Home() {
  const [name, setName] = useState("My 10-week reset");
  const [startDate, setStartDate] = useState("");
  const [lengthInWeeks, setLengthInWeeks] = useState("10");
  const [timeZoneId, setTimeZoneId] = useState("Atlantic/Reykjavik");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});
  const [submissionError, setSubmissionError] = useState<string | null>(null);
  const [createdCycle, setCreatedCycle] = useState<CycleDraft | null>(null);

  const errorsFor = (fieldName: string) => {
    const matchingKey = Object.keys(fieldErrors).find(
      (key) => key.toLowerCase() === fieldName.toLowerCase(),
    );
    return matchingKey ? fieldErrors[matchingKey] : undefined;
  };

  const submitCycle = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setIsSubmitting(true);
    setFieldErrors({});
    setSubmissionError(null);

    try {
      const response = await fetch(`${apiBaseUrl}/api/v1/cycles`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          name,
          startDate,
          timeZoneId,
          lengthInWeeks: Number(lengthInWeeks),
        }),
      });

      if (!response.ok) {
        const problem = (await response.json().catch(() => null)) as ValidationProblem | null;

        if (problem?.errors) {
          setFieldErrors(problem.errors);
          setSubmissionError("Check the highlighted details and try again.");
          return;
        }

        if (response.status === 401) {
          setSubmissionError("Sign-in is required before this cycle can be saved.");
          return;
        }

        setSubmissionError(problem?.detail ?? problem?.title ?? "The cycle could not be created.");
        return;
      }

      setCreatedCycle((await response.json()) as CycleDraft);
    } catch {
      setSubmissionError(
        "GameChanger could not reach the API. Make sure the local API is running and try again.",
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  const createAnother = () => {
    setCreatedCycle(null);
    setFieldErrors({});
    setSubmissionError(null);
  };

  return (
    <main className="min-h-screen px-5 py-5 sm:px-8 sm:py-8 lg:px-12">
      <div className="mx-auto grid min-h-[calc(100vh-2.5rem)] max-w-[1440px] overflow-hidden rounded-[2rem] border border-white/70 bg-white/85 shadow-[0_32px_90px_rgb(29_45_94/14%)] backdrop-blur sm:min-h-[calc(100vh-4rem)] lg:grid-cols-[minmax(310px,0.8fr)_minmax(540px,1.2fr)]">
        <aside className="relative overflow-hidden bg-[#14213d] px-7 py-8 text-white sm:px-10 sm:py-10 lg:px-12 lg:py-12">
          <div className="absolute -right-24 -top-24 h-64 w-64 rounded-full border-[46px] border-[#365cf5]/45" />
          <div className="absolute -bottom-28 -left-28 h-72 w-72 rounded-full border-[54px] border-[#ff6b4a]/25" />

          <div className="relative flex h-full flex-col">
            <div className="flex items-center gap-3">
              <span className="grid size-10 place-items-center rounded-xl bg-[#365cf5] text-base font-black tracking-tight">
                GC
              </span>
              <span className="text-lg font-semibold tracking-tight">GameChanger</span>
            </div>

            <div className="my-auto py-14 lg:py-10">
              <p className="mb-4 text-sm font-semibold uppercase tracking-[0.18em] text-[#9fb2ff]">
                Your next cycle
              </p>
              <h1 className="max-w-md text-4xl font-semibold leading-[1.08] tracking-[-0.045em] sm:text-5xl">
                Turn intention into a rhythm you can keep.
              </h1>
              <p className="mt-5 max-w-md text-base leading-7 text-slate-300">
                Start with a defined window. You will add goals and weekly reviews after the foundation is in place.
              </p>

              <ol className="mt-10 space-y-7">
                {setupSteps.map((step, index) => {
                  const Icon = step.icon;

                  return (
                    <li key={step.number} className="relative grid grid-cols-[44px_1fr] gap-4">
                      {index < setupSteps.length - 1 ? (
                        <span className="absolute left-[21px] top-11 h-[calc(100%+0.75rem)] w-px bg-white/15" />
                      ) : null}
                      <span className="relative z-10 grid size-11 place-items-center rounded-2xl border border-white/15 bg-white/10">
                        <Icon aria-hidden="true" className="size-5 text-[#9fb2ff]" />
                      </span>
                      <span>
                        <span className="block text-xs font-semibold tracking-[0.16em] text-slate-400">
                          {step.number}
                        </span>
                        <span className="mt-1 block text-base font-semibold">{step.title}</span>
                        <span className="mt-1 block max-w-sm text-sm leading-6 text-slate-400">
                          {step.detail}
                        </span>
                      </span>
                    </li>
                  );
                })}
              </ol>
            </div>

            <p className="relative text-sm text-slate-400">One focused cycle. One week at a time.</p>
          </div>
        </aside>

        <section className="flex items-center px-6 py-10 text-[#14213d] sm:px-12 lg:px-16 xl:px-24">
          <div className="mx-auto w-full max-w-xl">
            {createdCycle ? (
              <div aria-live="polite">
                <span className="grid size-14 place-items-center rounded-2xl bg-[#edf1ff] text-[#365cf5]">
                  <Check aria-hidden="true" className="size-7" strokeWidth={2.5} />
                </span>
                <p className="mt-7 text-sm font-semibold text-[#365cf5]">Draft created</p>
                <h2 className="mt-3 text-3xl font-semibold tracking-[-0.035em] text-[#14213d] sm:text-4xl">
                  {createdCycle.name}
                </h2>
                <p className="mt-3 text-base leading-7 text-slate-600">
                  The foundation is saved. Categories and goals will come next; the cycle will not begin until you start it.
                </p>

                <dl className="mt-8 grid gap-3 sm:grid-cols-2">
                  <div className="rounded-2xl border border-slate-200 bg-white p-4">
                    <dt className="text-sm text-slate-500">Timeline</dt>
                    <dd className="mt-1 font-semibold text-[#14213d]">
                      {formatDate(createdCycle.startDate)} – {formatDate(calculateEndDate(createdCycle.startDate, createdCycle.lengthInWeeks))}
                    </dd>
                  </div>
                  <div className="rounded-2xl border border-slate-200 bg-white p-4">
                    <dt className="text-sm text-slate-500">Duration</dt>
                    <dd className="mt-1 font-semibold text-[#14213d]">
                      {createdCycle.lengthInWeeks} {createdCycle.lengthInWeeks === 1 ? "week" : "weeks"}
                    </dd>
                  </div>
                  <div className="rounded-2xl border border-slate-200 bg-white p-4 sm:col-span-2">
                    <dt className="text-sm text-slate-500">Time zone</dt>
                    <dd className="mt-1 flex items-center justify-between gap-3 font-semibold text-[#14213d]">
                      <span>{createdCycle.timeZoneId}</span>
                      <span className="rounded-full bg-[#edf1ff] px-3 py-1 text-sm text-[#365cf5]">
                        {createdCycle.status}
                      </span>
                    </dd>
                  </div>
                </dl>

                <Button
                  type="button"
                  variant="outline"
                  size="lg"
                  className="mt-7 h-12 rounded-xl border-slate-300 bg-white px-5 text-base dark:bg-white dark:text-[#14213d]"
                  onClick={createAnother}
                >
                  <RotateCcw aria-hidden="true" className="size-4" />
                  Create another draft
                </Button>
              </div>
            ) : (
              <>
                <p className="text-sm font-semibold text-[#365cf5]">Step 1 of 3</p>
                <h2 className="mt-3 text-3xl font-semibold tracking-[-0.035em] text-[#14213d] sm:text-4xl">
                  Create your cycle
                </h2>
                <p className="mt-3 text-base leading-7 text-slate-600">
                  Give this period a name and a clear finish line. You can refine the details while it is still a draft.
                </p>

                <form className="mt-9 space-y-6" onSubmit={submitCycle}>
                  <div className="space-y-2.5">
                    <Label htmlFor="cycle-name">Cycle name</Label>
                    <Input
                      id="cycle-name"
                      name="name"
                      className="h-12 rounded-xl border-slate-200 bg-white px-4 text-base shadow-none dark:bg-white"
                      value={name}
                      onChange={(event) => setName(event.target.value)}
                      maxLength={200}
                      aria-invalid={Boolean(errorsFor("name"))}
                      aria-describedby={errorsFor("name") ? "cycle-name-error" : undefined}
                      required
                    />
                    {errorsFor("name") ? (
                      <p id="cycle-name-error" className="text-sm text-red-700">
                        {errorsFor("name")?.[0]}
                      </p>
                    ) : null}
                  </div>

                  <div className="grid gap-5 sm:grid-cols-2">
                    <div className="space-y-2.5">
                      <Label htmlFor="start-date">Start date</Label>
                      <Input
                        id="start-date"
                        name="startDate"
                        type="date"
                        className="h-12 rounded-xl border-slate-200 bg-white px-4 text-base shadow-none dark:bg-white"
                        value={startDate}
                        onChange={(event) => setStartDate(event.target.value)}
                        aria-invalid={Boolean(errorsFor("startDate"))}
                        aria-describedby={errorsFor("startDate") ? "start-date-error" : undefined}
                        required
                      />
                      {errorsFor("startDate") ? (
                        <p id="start-date-error" className="text-sm text-red-700">
                          {errorsFor("startDate")?.[0]}
                        </p>
                      ) : null}
                    </div>
                    <div className="space-y-2.5">
                      <Label htmlFor="cycle-length">Length in weeks</Label>
                      <Input
                        id="cycle-length"
                        name="lengthInWeeks"
                        type="number"
                        className="h-12 rounded-xl border-slate-200 bg-white px-4 text-base shadow-none dark:bg-white"
                        value={lengthInWeeks}
                        onChange={(event) => setLengthInWeeks(event.target.value)}
                        min={1}
                        max={52}
                        aria-invalid={Boolean(errorsFor("lengthInWeeks"))}
                        aria-describedby={errorsFor("lengthInWeeks") ? "cycle-length-error" : undefined}
                        required
                      />
                      {errorsFor("lengthInWeeks") ? (
                        <p id="cycle-length-error" className="text-sm text-red-700">
                          {errorsFor("lengthInWeeks")?.[0]}
                        </p>
                      ) : null}
                    </div>
                  </div>

                  <div className="space-y-2.5">
                    <Label htmlFor="time-zone">Time zone</Label>
                    <Input
                      id="time-zone"
                      name="timeZoneId"
                      className="h-12 rounded-xl border-slate-200 bg-white px-4 text-base shadow-none dark:bg-white"
                      value={timeZoneId}
                      onChange={(event) => setTimeZoneId(event.target.value)}
                      maxLength={100}
                      aria-invalid={Boolean(errorsFor("timeZoneId"))}
                      aria-describedby={errorsFor("timeZoneId") ? "time-zone-error" : "time-zone-hint"}
                      required
                    />
                    {errorsFor("timeZoneId") ? (
                      <p id="time-zone-error" className="text-sm text-red-700">
                        {errorsFor("timeZoneId")?.[0]}
                      </p>
                    ) : (
                      <p id="time-zone-hint" className="text-sm leading-6 text-slate-500">
                        Weekly boundaries follow this time zone, even when you travel.
                      </p>
                    )}
                  </div>

                  <div className="rounded-2xl border border-[#dfe5ff] bg-[#f4f6ff] p-4 text-sm leading-6 text-[#31416f]">
                    Your cycle will be saved as a draft. Nothing starts until you review the setup and choose to begin.
                  </div>

                  {submissionError ? (
                    <div
                      role="alert"
                      className="flex gap-3 rounded-2xl border border-red-200 bg-red-50 p-4 text-sm leading-6 text-red-800"
                    >
                      <AlertCircle aria-hidden="true" className="mt-0.5 size-5 shrink-0" />
                      <span>{submissionError}</span>
                    </div>
                  ) : null}

                  <Button
                    type="submit"
                    size="lg"
                    className="h-12 w-full rounded-xl bg-[#365cf5] text-base font-semibold shadow-[0_12px_28px_rgb(54_92_245/25%)] hover:bg-[#2949d3] sm:w-auto"
                    disabled={isSubmitting}
                  >
                    {isSubmitting ? (
                      <LoaderCircle aria-hidden="true" className="size-4 animate-spin" />
                    ) : (
                      <ArrowRight aria-hidden="true" className="size-4" />
                    )}
                    {isSubmitting ? "Creating draft…" : "Create draft cycle"}
                  </Button>
                </form>
              </>
            )}
          </div>
        </section>
      </div>
    </main>
  );
}
