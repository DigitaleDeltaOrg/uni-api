create extension postgis;

create table public.reference
(
  id bigint NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1 ),
  reference_type character varying(50) COLLATE pg_catalog."default" NOT NULL,
  organisation character varying(50) COLLATE pg_catalog."default" NOT NULL,
  code character varying(250) COLLATE pg_catalog."default" NOT NULL,
  uri character varying(2000) COLLATE pg_catalog."default",
  geometry geometry,
  display character varying(250) COLLATE pg_catalog."default",
  description character varying(250) COLLATE pg_catalog."default",
  external_key uuid NOT NULL,
  taxon_rank character varying(50) COLLATE pg_catalog."default",
  taxon_author character varying(250) COLLATE pg_catalog."default",
  taxon_parent_author character varying(250) COLLATE pg_catalog."default",
  cas_number character varying(20) COLLATE pg_catalog."default",
  parameter_type character varying(50) COLLATE pg_catalog."default",
  taxon_name_nl character varying(250) COLLATE pg_catalog."default",
  taxon_status_code character varying(10) COLLATE pg_catalog."default",
  taxon_parent_external_key uuid,
  taxon_type_external_key uuid,
  taxon_group_external_key uuid,
  CONSTRAINT reference_pkey PRIMARY KEY (id)
);
create index ic_reference_external_key on public.reference using btree (external_key);


create table public.observation (
  id uuid primary key not null,
  related_observation_id uuid,
  observation_type character varying(20) not null,
  phenomenon_time_start timestamp with time zone not null,
  phenomenon_time_end timestamp with time zone,
  valid_time_start timestamp with time zone not null,
  valid_time_end timestamp with time zone,
  observed_property_id bigint not null,
  observing_procedure_id bigint,
  host_id bigint,
  observer_id bigint,
  result_uom_id bigint,
  result_geometry geometry,
  result_count bigint,
  result_measure double precision,
  result_term character varying(50),
  result_vocab character varying(200),
  result_timeseries jsonb,
  result_complex jsonb,
  parameter jsonb,
  metadata jsonb,
  result_time timestamp with time zone not null,
  foi_id bigint,
  result_truth bit(1),
  result_text text,
  relation_role character varying(50),
  base_measure double precision,
  base_uom_id bigint,
  foreign key (foi_id) references public.reference (id)   				match simple on update no action on delete no action,
  foreign key (host_id) references public.reference (id)  				match simple on update no action on delete no action,
  foreign key (observed_property_id) references public.reference (id)   	match simple on update no action on delete no action,
  foreign key (observer_id) references public.reference (id)  				match simple on update no action on delete no action,
  foreign key (observing_procedure_id) references public.reference (id)  	match simple on update no action on delete no action,
  foreign key (related_observation_id) references public.observation (id)  	match simple on update no action on delete no action,
  foreign key (result_uom_id) references public.reference (id)  match simple on update no action on delete no action,
  foreign key (base_uom_id) references public.reference (id)  match simple on update no action on delete no action
);
create index observation_related_observation_id_index on public.observation using btree (related_observation_id);

